using In_Memory_Database.Classes.Dependencies.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.ObjectModel;
using System.Reflection;

namespace In_Memory_Database.Classes.Data
{
    public class DataTable :DefaultContractResolver, IDataTable
    {
        public string Name
        {
            get; set;
        }
        private List<Type> _columnTypes = [];
        private ISearchManager _searchManager;
        private List<string> _columnNames = [];
        private static readonly object tableOperationsLock = new();

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
        public ReadOnlyCollection<DataRow> Rows
        {
            get
            {
                return _rows.AsReadOnly();
            }
        }
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
                return Width + "x" + _rows.Count;
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
                return _rows.Count;
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

        //Only used when loading from disk, otherwise rows will not be initialized correctly
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
            }
            _rows = rows;
            foreach (string table in indexTables.Keys)
            {
                CreateIndex(table);
            }
        }

        public void AddColumn(string name, Type type)
        {
            lock (tableOperationsLock)
            {
                _columnTypes.Add(type);
                _columnNames.Add(name);
                foreach (var row in _rows)
                {
                    lock (tableOperationsLock)
                    {
                        row.Append(null);
                    }
                }
            }
        }

        public void RemoveColumn(string name)
        {
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
        }

        public void AddRow(DataRow newRow)
        {
            lock (tableOperationsLock)
            {
                if (newRow.Count != Width)
                {
                    throw new ArgumentException("Input doesn't match the table column's length");
                }
                //use a loop here to check beforehand if all the types match first
                for (int i = 0; i < Width; i++)
                {
                    if (newRow[i].GetType() != _columnTypes[i])
                    {
                        throw new ArgumentException("Input doesn't match the table column's type");
                    }
                }
                //add the row to current table and also index table
                _rows.Add(newRow);
                foreach (KeyValuePair<string, IndexTable> pair in _indexTables)
                {
                    int position = _columnNames.FindIndex((c) => pair.Key == c);
                    pair.Value.Insert(position, newRow);
                }
            }
        }

        public void RemoveRow(List<DataRow> toBeRemovedrows)
        {
            lock (tableOperationsLock)
            {
                if (toBeRemovedrows.Count == 0)
                {
                    return;
                }
                foreach (var row in toBeRemovedrows)
                {
                    _rows.Remove(row);
                }
                foreach (KeyValuePair<string, IndexTable> pair in _indexTables)
                {
                    int position = _columnNames.FindIndex((c) => pair.Key == c);
                    pair.Value.Delete(position, toBeRemovedrows[0]);
                }
            }
        }

        public void UpdateRow()
        {
        }

        public void ClearTable()
        {
            lock (tableOperationsLock)
            {
                _rows.Clear();
            }
        }

        public void CreateIndex(string targetColumn)
        {
            lock (tableOperationsLock)
            {
                for (int i = 0; i < Width; i++)
                {
                    if (_columnNames[i] == targetColumn)
                    {
                        var genericIndexTableType = typeof(IndexTable<>).MakeGenericType(
                            _columnTypes[i]
                        );
                        object indexTableInstance = Activator.CreateInstance(
                            genericIndexTableType,
                            i,
                            _rows
                        );
                        _indexTables.Add(targetColumn, (IndexTable)indexTableInstance);
                    }
                }
            }
        }

        public void DeleteIndex(string targetColumn)
        {
            lock (tableOperationsLock)
            {
                _indexTables.Remove(targetColumn);
            }
        }

        public List<DataRow> Search(SearchConditions conditions)
        {
            return _searchManager.Search(ColumnNames, Rows, conditions, IndexTables);
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
    }
}
