using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.ObjectModel;
using System.Reflection;
using Web_API_Database.Classes;

namespace In_Memory_Database.Classes
{
    public class DataTable :DefaultContractResolver
    {
        public string Name
        {
            get; set;
        }
        private List<Type> columnTypes = [];
        public ReadOnlyCollection<Type> ColumnTypes
        {
            get
            {
                return columnTypes.AsReadOnly();
            }
        }
        private List<string> columnNames = [];
        public ReadOnlyCollection<string> ColumnNames
        {
            get
            {
                return columnNames.AsReadOnly();
            }
        }
        private List<DataRow> rows = [];

        public ReadOnlyCollection<DataRow> Rows
        {
            get
            {
                return rows.AsReadOnly();
            }
        }
        private Dictionary<string, IndexTable> indexTables = [];
        public ReadOnlyDictionary<string, IndexTable> IndexTables
        {
            get
            {
                return indexTables.AsReadOnly();
            }
        }

        public string Size
        {
            get
            {
                return rows.Count + "x" + Width;
            }
        }
        public int Width
        {
            get
            {
                return columnTypes.Count;
            }
        }

        public DataTable(string tableName)
        {
            Name = tableName;
        }

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
            this.columnTypes = columnTypes;
            this.columnNames = columnNames;
            this.indexTables = indexTables;
            foreach (DataRow row in rows)
            {
                for (int i = 0; i < Width; i++)
                {
                    row[i] = Convert.ChangeType(row[i], columnTypes[i]);
                }
            }
            this.rows = rows;
        }

        public void AddColumn<T>(string name)
        {
            columnTypes.Add(typeof(T));
            columnNames.Add(name);
        }

        public void RemoveColumn(string name)
        {
            if (columnNames.Remove(name) == false)
            {
                throw new ArgumentException("Column doesn't exist");
            }
            indexTables.Remove(name);
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
                if (newRow[i].GetType() != columnTypes[i])
                {
                    throw new ArgumentException("Input doesn't match the table column's type");
                }
            }
            //add the row to current table and also index table
            rows.Add(newRow);
            foreach (KeyValuePair<string, IndexTable> pair in indexTables)
            {
                int position = columnNames.FindIndex((string c)=> pair.Key==c);
                pair.Value.Insert(position, newRow);
            }
        }

        public void RemoveRow(SearchConditions conditions)
        {
            List<DataRow> toBeRemovedrows = SearchManager.Get(
                this,
                conditions
            );

            if (toBeRemovedrows.Count == 0)
            {
                throw new ArgumentException("Couldn't find any row with that condition");
            }
            foreach (var row in toBeRemovedrows)
            {
                rows.Remove(row);
            }
            foreach (KeyValuePair<string, IndexTable> pair in indexTables)
            {
                int position = columnNames.FindIndex((string c)=> pair.Key==c);
                pair.Value.Delete(position, toBeRemovedrows[0]);
            }
        }

        public List<DataRow> Find(SearchConditions conditions)
        {
            return SearchManager.Get(this, conditions);
        }

        public void SaveToDisk(string dir) => FileManager.SaveToDisk(this, dir);

        public void LoadFromDisk(string dir)
        {
            FileManager.LoadFromDisk(dir);
        }

        public void StartTransaction()
        {
        }

        public void CommitTransaction()
        {
        }

        public void CreateIndex(string targetColumn)
        {
            for (int i = 0; i < this.Width; i++)
            {
                if (columnNames[i] == targetColumn)
                {
                    var genericIndexTableType = typeof(IndexTable<>).MakeGenericType(
                        columnTypes[i]
                    );
                    object indexTableInstance = Activator.CreateInstance(
                        genericIndexTableType,
                        i,
                        rows
                    );
                    indexTables.Add(targetColumn, (IndexTable)indexTableInstance);
                }
            }
        }

        public void RemoveIndex()
        {
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var result = base.GetSerializableMembers(objectType);
            if (objectType == typeof(DataTable))
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
