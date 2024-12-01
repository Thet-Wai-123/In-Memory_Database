using System.Collections.ObjectModel;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Web_API_Database.Classes;

namespace In_Memory_Database.Classes
{
    public class DataTable : DefaultContractResolver
    {
        public string Name { get; set; }
        private List<Type> columnTypes = [];
        public ReadOnlyCollection<Type> ColumnTypes
        {
            get { return columnTypes.AsReadOnly(); }
        }
        private List<string> columnNames = [];
        public ReadOnlyCollection<string> ColumnNames
        {
            get { return columnNames.AsReadOnly(); }
        }

        private Dictionary<Guid, DataRow> rows = [];

        public ReadOnlyDictionary<Guid, DataRow> Rows
        {
            get { return rows.AsReadOnly(); }
        }

        public string Size
        {
            get { return rows.Count + "x" + Width; }
        }
        public int Width
        {
            get { return columnTypes.Count; }
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
            Dictionary<Guid, DataRow> rows
        )
        {
            Name = name;
            this.columnTypes = columnTypes;
            this.columnNames = columnNames;
            foreach (DataRow row in rows.Values)
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
        }

        public void AddRow(DataRow values)
        {
            if (values.Count != Width)
            {
                throw new ArgumentException("Input doesn't match the table column's length");
            }
            //use a loop here to check beforehand if all the types match first
            for (int i = 0; i < Width; i++)
            {
                if (values[i].GetType() != columnTypes[i])
                {
                    throw new ArgumentException("Input doesn't match the table column's type");
                }
            }
            //add the row to the table
            rows[Guid.NewGuid()] = values;
        }

        public void RemoveRow(SearchConditions conditions)
        {
            List<Guid> toBeRemovedrows = SearchManager.SearchTableForIdUsingConditions(
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
        }

        public List<DataRow> Find(SearchConditions conditions)
        {
            List<Guid> rowsIds = SearchManager.SearchTableForIdUsingConditions(this, conditions);
            List<DataRow> foundrows = new();
            foreach (Guid id in rowsIds)
            {
                foundrows.Add(rows[id]);
            }
            return foundrows;
        }

        public void SaveToDisk(string dir) => FileManager.SaveToDisk(this, dir);

        public void ReadFromDisk() { }

        public void StartTransaction() { }

        public void CommitTransaction() { }

        public void CreateIndex() { }

        public void RemoveIndex() { }

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
