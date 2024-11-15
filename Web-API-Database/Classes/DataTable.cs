using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;

namespace In_Memory_Database.Classes
{
    public class DataTable
    {
        public string TableName { get; set; }
        public List<Type> ColumnsType { get; set; } = [];
        private List<string> ColumnsName { get; set; } = [];
        private readonly Dictionary<Guid, List<dynamic>> rows = [];
        public string Size
        {
            get { return rows.Count + "x" + ColumnsType.Count; }
        }
        public int Width
        {
            get { return ColumnsType.Count; }
        }

        public DataTable(string tableName)
        {
            TableName = tableName;
        }

        public void AddColumn<T>(string name)
        {
            ColumnsType.Add(typeof(T));
            ColumnsName.Add(name);
        }

        public void RemoveColumn(string name)
        {
            if (ColumnsName.Remove(name) == false)
            {
                throw new ArgumentException("Column doesn't exist");
            }
        }

        public void AddRow(List<dynamic> values)
        {
            if (values.Count != ColumnsType.Count)
            {
                throw new ArgumentException("Input doesn't match the table column's length");
            }
            //use a loop here to check beforehand if all the types match first
            for (int i = 0; i < ColumnsType.Count; i++)
            {
                if (values[i].GetType() != ColumnsType[i])
                {
                    throw new ArgumentException("Input doesn't match the table column's type");
                }
            }

            //add the row to the table
            rows[Guid.NewGuid()] = values;
        }

        public void RemoveRow() { }

        //public void getTableSummary() { }

        public void StartTransaction() { }

        public void CommitTransaction() { }

        public void SaveToDisk() => FileManager.SaveToDisk(rows, "./DataTable/rows.txt");

        public void ReadFromDisk() { }

        public void CreateIndex() { }

        public void RemoveIndex() { }

        //Gonna need a lot of parameters here to query based on column names, have equality checks
        public void query() { }
    }
}
