using System.Collections.ObjectModel;
using In_Memory_Database.Classes.Dependencies.Managers;

namespace In_Memory_Database.Classes.Data
{
    public class Database
    {
        private Dictionary<string, DataTable> _tables = new();
        public ReadOnlyDictionary<string, DataTable> Tables
        {
            get { return _tables.AsReadOnly(); }
        }
        public readonly ISearchManager _searchManager;
        public readonly IDiskManager _diskManager;

        private string diskLocation = "./storage";

        public Database(ISearchManager searchManager, IDiskManager diskManager)
        {
            _searchManager = searchManager;
            _diskManager = diskManager;
        }

        public void CreateTable(
            string tableName,
            List<string> columnNames,
            List<Type> columnTypes,
            List<DataRow> rows = null
        )
        {
            _tables.Add(
                tableName,
                new DataTable(tableName, columnNames, columnTypes, _searchManager)
            );

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    _tables[tableName].AddRow(row);
                }
            }
        }

        public DataTable this[string tableName] =>
            Tables.TryGetValue(tableName, out DataTable table)
                ? table
                : throw new KeyNotFoundException();

        public void SaveToDisk()
        {
            _diskManager.SaveTodisk(this, diskLocation);
        }

        public void LoadFromDisk()
        {
            _diskManager.LoadFromDisk(this, diskLocation);
        }

        public void ClearDb()
        {
            _tables.Clear();
        }

        public void SetDiskLocation(string location)
        {
            diskLocation = location;
        }
    }
}
