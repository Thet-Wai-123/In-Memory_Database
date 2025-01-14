using Microsoft.Extensions.DependencyInjection.Extensions;

namespace In_Memory_Database.Classes
{
    public class Database
    {
        public List<DataTable> tables;
        public IReadOnlyCollection<DataTable> Tables => tables.AsReadOnly();

        private readonly ISearchManager _searchManager;
        private readonly IFileManager _fileManager;
        public readonly string storageLocation = "../storage/";

        public Database(ISearchManager searchManager, IFileManager fileManager)
        {
            tables = new List<DataTable>();
            _searchManager = searchManager;
            _fileManager = fileManager;
        }

        public void CreateTable(string tableName, List<string> columnNames, List<Type> columnTypes)
        {
            tables.Add(new DataTable(tableName, columnNames, columnTypes));
        }

        public void AddRow(string tableName, DataRow row)
        {
            var table = GetTable(tableName);
            table.AddRow((DataRow)row);
        }

        public void DeleteRows(string tableName, string columnName, string op, string value)
        {
            SearchConditions conditions = new(columnName, op, value);
            var table = GetTable(tableName);
            var toBeRemovedRows = _searchManager.Search(
                table.ColumnNames,
                table.Rows,
                conditions,
                table.IndexTables
            );
            table.RemoveRow(toBeRemovedRows);
        }

        public DataTable GetTable(string tableName)
        {
            return tables.Find(t => t.Name == tableName);
        }

        public List<DataRow> Filter(DataTable table, string columnName, string op, string value)
        {
            SearchConditions conditions = new(columnName, op, value);
            return _searchManager.Search(
                table.ColumnNames,
                table.Rows,
                conditions,
                table.IndexTables
            );
        }

        public void CreateIndex(string tableName, string columnName)
        {
            var table = GetTable(tableName);
            table.CreateIndex(columnName);
        }

        public void DeleteIndex(string tableName, string columnName)
        {
            var table = GetTable(tableName);
            table.DeleteIndex(columnName);
        }

        public void SaveToDisk()
        {
            _fileManager.SaveTodisk(this, storageLocation);
        }

        public void LoadFromDisk()
        {
            tables = _fileManager.LoadFromDisk(storageLocation);
        }

        public void ClearDb()
        {
            tables.Clear();
        }
    }
}
