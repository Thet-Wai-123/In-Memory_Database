using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Web_API_Database.Classes;

namespace In_Memory_Database.Classes
{
    public class DataTable
    {
        public string TableName { get; set; }
        public List<Type> ColumnTypes { get; set; } = [];
        public List<string> ColumnNames { get; set; } = [];
        public Dictionary<Guid, List<dynamic>> Rows { get; } = [];
        public string Size
        {
            get { return Rows.Count + "x" + ColumnTypes.Count; }
        }
        public int Width
        {
            get { return ColumnTypes.Count; }
        }

        public DataTable(string tableName)
        {
            TableName = tableName;
        }

        public void AddColumn<T>(string name)
        {
            ColumnTypes.Add(typeof(T));
            ColumnNames.Add(name);
        }

        public void RemoveColumn(string name)
        {
            if (ColumnNames.Remove(name) == false)
            {
                throw new ArgumentException("Column doesn't exist");
            }
        }

        public void AddRow(List<dynamic> values)
        {
            if (values.Count != ColumnTypes.Count)
            {
                throw new ArgumentException("Input doesn't match the table column's length");
            }
            //use a loop here to check beforehand if all the types match first
            for (int i = 0; i < ColumnTypes.Count; i++)
            {
                if (values[i].GetType() != ColumnTypes[i])
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

        public List<dynamic> Find(SearchConditions conditions)
        {
            List<Guid> rowsIds = SearchManager.SearchTableForIdUsingConditions(this, conditions);
            List<dynamic> foundRows = new();
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
    }
}
