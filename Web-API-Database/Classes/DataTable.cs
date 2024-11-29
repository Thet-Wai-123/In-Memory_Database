using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
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
            init { columnTypes = value.ToList(); }
        }
        private List<string> columnNames = [];
        public ReadOnlyCollection<string> ColumnNames
        {
            get { return columnNames.AsReadOnly(); }
            init { columnNames = value.ToList(); }
        }

        public Dictionary<Guid, DataRow> Rows { get; } = [];
        public string Size
        {
            get { return Rows.Count + "x" + columnTypes.Count; }
        }
        public int Width
        {
            get { return columnTypes.Count; }
        }

        public DataTable(string tableName)
        {
            Name = tableName;
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
            if (values.Count != columnTypes.Count)
            {
                throw new ArgumentException("Input doesn't match the table column's length");
            }
            //use a loop here to check beforehand if all the types match first
            for (int i = 0; i < columnTypes.Count; i++)
            {
                if (values[i].GetType() != columnTypes[i])
                {
                    throw new ArgumentException("Input doesn't match the table column's type");
                }
            }
            //add the row to the table
            Rows[Guid.NewGuid()] = values;
        }

        public void RemoveRow(SearchConditions conditions)
        {
            List<Guid> toBeRemovedRows = SearchManager.SearchTableForIdUsingConditions(
                this,
                conditions
            );

            if (toBeRemovedRows.Count == 0)
            {
                throw new ArgumentException("Couldn't find any row with that condition");
            }
            foreach (var row in toBeRemovedRows)
            {
                Rows.Remove(row);
            }
        }

        public List<DataRow> Find(SearchConditions conditions)
        {
            List<Guid> rowsIds = SearchManager.SearchTableForIdUsingConditions(this, conditions);
            List<DataRow> foundRows = new();
            foreach (Guid id in rowsIds)
            {
                foundRows.Add(Rows[id]);
            }
            return foundRows;
        }

        public void SaveToDisk() => FileManager.SaveToDisk(this, "./DataTable/rows.txt");

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
