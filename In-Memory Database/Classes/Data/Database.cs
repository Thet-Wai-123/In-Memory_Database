using System.Collections.ObjectModel;
using In_Memory_Database.Classes.Dependencies.Managers;

namespace In_Memory_Database.Classes.Data
{
    public class Database
    {
        private Dictionary<string, DataTable> _tables = new();
        private readonly ISearchManager _searchManager;
        private readonly IFileManager _fileManager;

        public readonly string storageLocation = "../storage/";

        public ReadOnlyDictionary<string, DataTable> Tables
        {
            get { return _tables.AsReadOnly(); }
        }

        public Database(ISearchManager searchManager, IFileManager fileManager)
        {
            _searchManager = searchManager;
            _fileManager = fileManager;
        }

        public void CreateTable(string tableName, List<string> columnNames, List<Type> columnTypes)
        {
            _tables.Add(
                tableName,
                new DataTable(tableName, columnNames, columnTypes, _searchManager)
            );
        }

        //public void AddRow(string tableName, List<object> row)
        //{
        //    var table = GetTable(tableName);
        //    table.AddRow((DataRow)row);
        //}

        //public void DeleteRows(string tableName, string columnName, string op, string value)
        //{
        //    SearchConditions conditions = new(columnName, op, value);
        //    var table = GetTable(tableName);
        //    var toBeRemovedRows = _searchManager.Search(
        //        table.ColumnNames,
        //        table.Rows,
        //        conditions,
        //        table.IndexTables
        //    );
        //    table.RemoveRow(toBeRemovedRows);
        //}

        //public DataTable GetTable(string tableName)
        //{
        //    return tables.Find(t => t.Name == tableName);
        //}
        public DataTable this[string tableName] =>
            Tables.TryGetValue(tableName, out DataTable table)
                ? table
                : throw new KeyNotFoundException();

        //public List<DataRow> Filter(DataTable table, string columnName, string op, string value)
        //{
        //    SearchConditions conditions = new(columnName, op, value);
        //    return _searchManager.Search(
        //        table.ColumnNames,
        //        table.Rows,
        //        conditions,
        //        table.IndexTables
        //    );
        //}

        //public void CreateIndex(string tableName, string columnName)
        //{
        //    var table = GetTable(tableName);
        //    table.CreateIndex(columnName);
        //}

        //public void DeleteIndex(string tableName, string columnName)
        //{
        //    var table = GetTable(tableName);
        //    table.DeleteIndex(columnName);
        //}

        public void SaveToDisk()
        {
            _fileManager.SaveTodisk(this, storageLocation);
        }

        public void LoadFromDisk()
        {
            _tables = _fileManager.LoadFromDisk(storageLocation);
        }

        public void ClearDb()
        {
            _tables.Clear();
        }
    }
}
