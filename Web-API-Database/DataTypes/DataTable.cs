using System.Text.Json.Nodes;

namespace Web_API_Database.DataTypes
{
    public class DataTable
    {
        private string tableName;
        private List<Type> columnsType;
        private List<string> columnsName;
        private readonly Dictionary<Guid, List<dynamic>> rows;

        public DataTable(string tableName)
        {
            this.tableName = tableName;
        }

        public void AddColumn<T>(string name)
        {
            columnsType.Add(typeof(T));
            columnsName.Add(name);
        }

        //public void RemoveColumn(string name)
        //{
        //    try
        //    {
        //        foreach()
        //    }
        //}

        public void AddRow(List<dynamic> values)
        {
            try
            {
                if (values.Count != columnsType.Count)
                {
                    throw new Exception("Input doesn't match the table column's length");
                }
                //use a loop here to check beforehand if all the types match first
                for (int i = 0; i < columnsType.Count; i++)
                {
                    if (values[i].GetType() != columnsType[i])
                    {
                        throw new Exception("Input doesn't match the table column's type");
                    }
                }

                //add the row to the table
                rows[Guid.NewGuid()] = values;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void StartTransaction() { }

        public void CommitTransaction() { }

        public void WriteToDisk() { }

        public void ReadFromDisk() { }

        public void CreateIndex() { }

        public void RemoveIndex() { }

        //Gonna need a lot of parameters here to query based on column names, have equality checks
        public void query() { }
    }
}
