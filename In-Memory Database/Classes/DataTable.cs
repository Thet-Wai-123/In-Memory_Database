using System.Collections.ObjectModel;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace In_Memory_Database.Classes
{
    public class DataTable : DefaultContractResolver, IDataTable
    {
        public string Name { get; set; }
        private List<Type> _columnTypes = [];
        public ReadOnlyCollection<Type> ColumnTypes
        {
            get { return _columnTypes.AsReadOnly(); }
        }
        private List<string> _columnNames = [];
        public ReadOnlyCollection<string> ColumnNames
        {
            get { return _columnNames.AsReadOnly(); }
        }
        private List<DataRow> _rows = [];

        public ReadOnlyCollection<DataRow> Rows
        {
            get { return _rows.AsReadOnly(); }
        }
        private Dictionary<string, IndexTable> _indexTables = [];
        public ReadOnlyDictionary<string, IndexTable> IndexTables
        {
            get { return _indexTables.AsReadOnly(); }
        }

        public string Size
        {
            get { return Width + "x" + _rows.Count; }
        }
        public int Width
        {
            get { return _columnTypes.Count; }
        }
        public int Height
        {
            get { return _rows.Count; }
        }

        private ISearchManager _searchManager;

        public DataTable(string tableName, ISearchManager searchManager)
        {
            Name = tableName;
            _searchManager = searchManager;
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

        //Only used when loading from disk
        [JsonConstructor]
        public DataTable(
            string name,
            List<Type> columnTypes,
            List<string> columnNames,
            List<DataRow> rows,
            Dictionary<string, IndexTable> indexTables
        )
        {
            Name = name;
            _columnTypes = columnTypes;
            _columnNames = columnNames;
            _indexTables = indexTables;
            _searchManager = ServiceProviderFactory.ServiceProvider.GetService<ISearchManager>();

            //Without this, after deserializing back, the state will not be exactly the same, as the default type will be set to dynamic.
            foreach (DataRow row in rows)
            {
                for (int i = 0; i < Width; i++)
                {
                    row[i] = Convert.ChangeType(row[i], columnTypes[i]);
                }
            }
            _rows = rows;
        }

        public void AddColumn(string name, Type type)
        {
            _columnTypes.Add(type);
            _columnNames.Add(name);
        }

        public void RemoveColumn(string name)
        {
            if (_columnNames.Remove(name) == false)
            {
                throw new ArgumentException("Column doesn't exist");
            }
            _indexTables.Remove(name);
        }

        public void AddRow(DataRow newRow)
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
                int position = _columnNames.FindIndex((string c) => pair.Key == c);
                pair.Value.Insert(position, newRow);
            }
        }

        public void RemoveRow(SearchConditions conditions)
        {
            List<DataRow> toBeRemovedrows = _searchManager.Search(
                ColumnNames,
                Rows,
                conditions,
                IndexTables
            );

            if (toBeRemovedrows.Count == 0)
            {
                throw new ArgumentException("Couldn't find any row with that condition");
            }
            foreach (var row in toBeRemovedrows)
            {
                _rows.Remove(row);
            }
            foreach (KeyValuePair<string, IndexTable> pair in _indexTables)
            {
                int position = _columnNames.FindIndex((string c) => pair.Key == c);
                pair.Value.Delete(position, toBeRemovedrows[0]);
            }
        }

        public List<DataRow> Get(SearchConditions conditions, bool useIndex = true)
        {
            return _searchManager.Search(ColumnNames, Rows, conditions, IndexTables, useIndex);
        }

        public void ClearTable()
        {
            _rows.Clear();
        }

        public void SaveToDisk(string dir) => FileManager.SaveToDisk(this, dir);

        public void LoadFromDisk(string dir)
        {
            FileManager.LoadFromDisk(dir);
        }

        public void StartTransaction() { }

        public void CommitTransaction() { }

        public void CreateIndex(string targetColumn)
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

        public void RemoveIndex() { }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
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
