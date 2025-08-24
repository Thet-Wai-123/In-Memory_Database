using DotNext.Collections.Generic;
using In_Memory_Database.Classes.Dependencies.Managers;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;

namespace In_Memory_Database.Classes.Data
{
    public class DataTable :DefaultContractResolver, IDataTable
    {
        public string Name
        {
            get; set;
        }
        private readonly List<Type> _columnTypes = [];
        private ISearchManager _searchManager;
        private readonly List<string> _columnNames = [];

        //this lock will be used to do any operation here, and then released immediately after its done. For ongoing transaction, there will be another shared lock from LockManager
        private readonly object tableOperationsLock = new();
        public ReadOnlyCollection<Type> ColumnTypes
        {
            get
            {
                return _columnTypes.AsReadOnly();
            }
        }
        public ReadOnlyCollection<string> ColumnNames
        {
            get
            {
                return _columnNames.AsReadOnly();
            }
        }
        private List<DataRow> _rows = [];

        [JsonProperty]
        public ReadOnlyCollection<DataRow> Rows
        {
            get
            {
                lock (tableOperationsLock)
                {
                    return FilterByCommited(_rows).AsReadOnly();
                }
            }
        }

        private List<DataRow> FilterByCommited(List<DataRow> rows)
        {
            List<DataRow> result = new();
            var curTransaction = TransactionManager.GetCurrentTransaction();
            var xid = curTransaction != null ? curTransaction.xid : TransactionManager.PeekTopId();
            foreach (var row in rows)
            {
                var version = _rowsVersions[row];
                //(xmin is committed AND xmax is not committed) OR (xmin == xid < xmax)
                if (
                    (
                        TransactionManager.checkTransactionStatus(version.Xmin)
                        && !TransactionManager.checkTransactionStatus(version.Xmax)
                    ) || (xid == version.Xmin && xid < version.Xmax)
                )
                {
                    result.Add(row);
                }
            }
            return result;
        }

        private Dictionary<DataRow, DataRowVersion> _rowsVersions = [];

        private Dictionary<string, IndexTable> _indexTables = [];
        public ReadOnlyDictionary<string, IndexTable> IndexTables
        {
            get
            {
                return _indexTables.AsReadOnly();
            }
        }
        public string Size
        {
            get
            {
                return Width + "x" + Height;
            }
        }
        public int Width
        {
            get
            {
                return _columnTypes.Count;
            }
        }
        public int Height
        {
            get
            {
                return Rows.Count;
            }
        }

        public DataTable(
            string tableName,
            List<string> columnNames,
            List<Type> columnTypes,
            ISearchManager searchManager
        )
        {
            Name = tableName;
            _columnNames = columnNames;
            _columnTypes = columnTypes;
            _searchManager = searchManager;
        }

        //Only used when loading from disk, otherwise rows will not be initialized correctly. Here we assume the rows version to be 0, to essentially reset them
        [JsonConstructor]
        public DataTable(
            string name,
            List<Type> columnTypes,
            List<string> columnNames,
            List<DataRow> rows,
            Dictionary<String, object> indexTables
        )
        {
            Name = name;
            _columnTypes = columnTypes;
            _columnNames = columnNames;
            //Without this, after deserializing back, the state will not be exactly the same, as the default type will be set to dynamic.
            foreach (DataRow row in rows)
            {
                for (int i = 0; i < Width; i++)
                {
                    row[i] = Convert.ChangeType(row[i], columnTypes[i]);
                }
                _rowsVersions.Add(row, new DataRowVersion(0, long.MaxValue));
            }
            _rows = rows;

            foreach (string column in indexTables.Keys)
            {
                CreateIndex(column);
            }
        }

        public async Task AddColumn(string name, Type type, object? defaultValue = null)
        {
            try
            {
                if (TransactionManager.GetCurrentTransaction() != null)
                    throw new InvalidOperationException(
                        "This method is not supported inside explicitly called transaction"
                    );

                var xid = TransactionManager.Begin();
                if (defaultValue != null && defaultValue?.GetType() != type)
                {
                    throw new ArgumentException(
                        "Wrong default value passed into creating new column"
                    );
                }

                await LockManager.GetLock(LockManager.LockType.AccessExclusiveLock, this, xid);
                lock (tableOperationsLock)
                {
                    _columnTypes.Add(type);
                    _columnNames.Add(name);
                    foreach (var row in _rows)
                    {
                        row.Add(defaultValue);
                    }
                }
                TransactionManager.Commit();
            }
            catch
            {
                TransactionManager.RollBack();
                throw;
            }
        }

        public async Task RemoveColumn(string name)
        {
            try
            {
                if (TransactionManager.GetCurrentTransaction() != null)
                    throw new InvalidOperationException(
                        "This method is not supported inside explicitly called transaction"
                    );
                var xid = TransactionManager.Begin();

                await LockManager.GetLock(LockManager.LockType.AccessExclusiveLock, this, xid);
                lock (tableOperationsLock)
                {
                    int index = _columnNames.IndexOf(name);
                    if (index == -1)
                    {
                        throw new ArgumentException("Column doesn't exist");
                    }

                    _columnNames.RemoveAt(index);
                    _columnTypes.RemoveAt(index);

                    foreach (var row in _rows)
                    {
                        row.RemoveAt(index);
                    }

                    _indexTables.Remove(name);
                }
                TransactionManager.Commit();
            }
            catch
            {
                TransactionManager.RollBack();
                throw;
            }
        }

        public async Task<ReadOnlyCollection<DataRow>> Search(SearchConditions conditions)
        {
            long xid = 0;
            if (TransactionManager.GetCurrentTransaction() == null)
                xid = TransactionManager.PeekTopId();
            else
                xid = TransactionManager.GetCurrentTransaction().xid;

            await LockManager.GetLock(LockManager.LockType.AccessShareLock, this, xid);
            var unfilteredRows = _searchManager.Search(ColumnNames, _rows, conditions, IndexTables);
            return FilterByCommited(unfilteredRows).AsReadOnly();
        }

        public async Task AddRow(DataRow newRow)
        {
            try
            {
                var hasOnGoingTransaction = TransactionManager.GetCurrentTransaction() != null;
                if (!hasOnGoingTransaction)
                    TransactionManager.Begin();

                var xid = TransactionManager.GetCurrentTransaction().xid;

                await LockManager.GetLock(LockManager.LockType.RowExclusiveLock, this, xid, newRow);

                lock (tableOperationsLock)
                {
                    if (newRow.Count != Width)
                    {
                        throw new ArgumentException(
                            "Input doesn't match the table column's length"
                        );
                    }
                    //use a loop here to check beforehand if all the types match first
                    for (int i = 0; i < Width; i++)
                    {
                        if (newRow[i] != null && newRow[i].GetType() != _columnTypes[i])
                        {
                            throw new ArgumentException(
                                "Input doesn't match the table column's type"
                            );
                        }
                    }
                    //add the row to current table and also index table
                    _rows.Add(newRow);
                    foreach (KeyValuePair<string, IndexTable> pair in _indexTables)
                    {
                        int position = _columnNames.FindIndex((c) => pair.Key == c);
                        pair.Value.Insert(position, newRow);
                    }
                    _rowsVersions.Add(newRow, new DataRowVersion(xid, long.MaxValue));
                }

                if (!hasOnGoingTransaction)
                    TransactionManager.Commit();
            }
            catch
            {
                TransactionManager.RollBack();
                throw;
            }
        }

        public async Task RemoveRow(SearchConditions searchConditions)
        {
            try
            {
                var hasOnGoingTransaction = TransactionManager.GetCurrentTransaction() != null;
                if (!hasOnGoingTransaction)
                    TransactionManager.Begin();

                var xid = TransactionManager.GetCurrentTransaction().xid;

                var toBeRemovedrows = await this.Search(searchConditions);

                if (toBeRemovedrows.Count == 0)
                {
                    return;
                }
                foreach (var removedRow in toBeRemovedrows)
                    await LockManager.GetLock(
                        LockManager.LockType.RowExclusiveLock,
                        this,
                        xid,
                        removedRow
                    );

                lock (tableOperationsLock)
                {
                    foreach (var removedRow in toBeRemovedrows)
                    {
                        _rowsVersions[removedRow].Xmax = xid;
                    }

                    if (!hasOnGoingTransaction)
                        TransactionManager.Commit();
                }
            }
            catch
            {
                TransactionManager.RollBack();
                throw;
            }
        }

        public async Task UpdateRow(
            SearchConditions searchConditions,
            string column,
            dynamic newValue
        )
        {
            try
            {
                var hasOnGoingTransaction = TransactionManager.GetCurrentTransaction() != null;
                if (!hasOnGoingTransaction)
                    TransactionManager.Begin();

                if (!_columnNames.Contains(column))
                {
                    return;
                }
                var columnIndex = _columnNames.FindIndex(x => x == column);
                if (newValue.GetType() != ColumnTypes[columnIndex])
                {
                    throw new ArgumentException("Input doesn't match the table column's type");
                }
                var xid = TransactionManager.GetCurrentTransaction().xid;
                var candidateRows = await this.Search(searchConditions);

                var lockTasks = candidateRows.Select(row =>
                    LockManager.GetLock(LockManager.LockType.RowExclusiveLock, this, xid, row)
                );

                await Task.WhenAll(lockTasks);

                //Search again after locking to confirm that the condition still holds true
                var foundRows = await this.Search(searchConditions);

                lock (tableOperationsLock)
                {
                    foreach (DataRow oldRow in foundRows)
                    {
                        _rowsVersions[oldRow].Xmax = xid;

                        DataRow newRow = new(oldRow);
                        newRow[columnIndex] = newValue;
                        _rows.Add(newRow);
                        foreach (KeyValuePair<string, IndexTable> pair in _indexTables)
                        {
                            int position = _columnNames.FindIndex((c) => pair.Key == c);
                            pair.Value.Insert(position, newRow);
                        }
                        _rowsVersions.Add(newRow, new DataRowVersion(xid, long.MaxValue));
                    }
                }

                if (!hasOnGoingTransaction)
                    TransactionManager.Commit();
            }
            catch
            {
                TransactionManager.RollBack();
                throw;
            }
        }

        public async Task ClearTable()
        {
            try
            {
                if (TransactionManager.GetCurrentTransaction() != null)
                    throw new InvalidOperationException(
                        "This method is not supported inside explicitly called transaction"
                    );
                var xid = TransactionManager.Begin();

                await LockManager.GetLock(LockManager.LockType.AccessExclusiveLock, this, xid);

                lock (tableOperationsLock)
                {
                    foreach (var row in Rows)
                    {
                        _rowsVersions[row].Xmax = xid;
                    }
                    _indexTables.Clear();
                }

                TransactionManager.Commit();
            }
            catch
            {
                TransactionManager.RollBack();
                throw;
            }
        }

        public async Task VacuumInactiveRows()
        {
            try
            {
                if (TransactionManager.GetCurrentTransaction() != null)
                    throw new InvalidOperationException(
                        "This method is not supported inside explicitly called transaction"
                    );
                var xid = TransactionManager.Begin();

                await LockManager.GetLock(LockManager.LockType.AccessExclusiveLock, this, xid);

                //Keep track of this for indexTable to also delete inactive nodes.
                List<DataRow> inactiveRows = new();

                lock (tableOperationsLock)
                {
                    foreach (var row in _rows)
                    {
                        var version = _rowsVersions[row];
                        if (
                            !(
                                TransactionManager.checkTransactionStatus(version.Xmin)
                                && !TransactionManager.checkTransactionStatus(version.Xmax)
                            ) && !(xid == version.Xmin && xid < version.Xmax)
                        )
                        {
                            inactiveRows.Add(row);
                        }
                    }
                    //Deleting the nodes in the index table
                    for (int i = 0; i < Width; i++)
                    {
                        if (_indexTables.ContainsKey(_columnNames[i]))
                        {
                            foreach (var inactiveRow in inactiveRows)
                                _indexTables[_columnNames[i]].Delete(i, inactiveRow);
                        }
                    }

                    //Deletes all the non-active versions in _rows
                    _rows = FilterByCommited(_rows).ToList();
                }

                TransactionManager.Commit();
            }
            catch
            {
                TransactionManager.RollBack();
                throw;
            }
        }

        public async Task CreateIndex(string targetColumn)
        {
            try
            {
                if (TransactionManager.GetCurrentTransaction() != null)
                    throw new Exception(
                        "This method is not supported inside explicitly called transaction"
                    );
                var xid = TransactionManager.Begin();

                await LockManager.GetLock(LockManager.LockType.AccessExclusiveLock, this, xid);

                lock (tableOperationsLock)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        if (_columnNames[i] == targetColumn)
                        {
                            var genericIndexTableType = typeof(IndexTable<>).MakeGenericType(
                                _columnTypes[i]
                            );
                            object? indexTableInstance = Activator.CreateInstance(
                                genericIndexTableType,
                                i,
                                _rows
                            );
                            if (indexTableInstance != null)
                                _indexTables.Add(targetColumn, (IndexTable)indexTableInstance);
                            else
                                throw new Exception("Something wrong with creating new index");
                        }
                    }
                }

                TransactionManager.Commit();
            }
            catch
            {
                TransactionManager.RollBack();
                throw;
            }
        }

        public async Task DeleteIndex(string targetColumn)
        {
            try
            {
                if (TransactionManager.GetCurrentTransaction() != null)
                    throw new Exception(
                        "This method is not supported inside explicitly called transaction"
                    );
                var xid = TransactionManager.Begin();

                await LockManager.GetLock(LockManager.LockType.AccessExclusiveLock, this, xid);

                lock (tableOperationsLock)
                {
                    _indexTables.Remove(targetColumn);
                }

                TransactionManager.Commit();
            }
            catch
            {
                TransactionManager.RollBack();
                throw;
            }
        }

        public void SetSearchManager(ISearchManager searchManager)
        {
            _searchManager = searchManager;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            lock (tableOperationsLock)
            {
                var result = base.GetSerializableMembers(objectType);
                if (objectType == typeof(IDataTable))
                {
                    var memberInfo = objectType
                        .GetMember("_myField", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Single();
                    result.Add(memberInfo);
                }
                return result;
            }
        }

        //These 2 methods belwo are internal for for making sure vacuum method works
        internal int getRowsLengthIncludeHidden()
        {
            return _rows.Count();
        }

        internal int getIndexTableLength(string indexName)
        {
            return _indexTables[indexName].GetLength();
        }
    }
}
