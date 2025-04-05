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

        private string _diskLocation;
        private bool _saveOnDispose;

        public Database(
            ISearchManager searchManager,
            IDiskManager diskManager,
            bool saveOnDispose = true,
            string diskLocation = "./backup"
        )
        {
            _searchManager = searchManager;
            _diskManager = diskManager;
            _diskLocation = diskLocation;

            if (saveOnDispose)
            {
                //Save on exit and unhandled exception
                AppDomain.CurrentDomain.ProcessExit += (s, e) => SaveToDisk();
                AppDomain.CurrentDomain.UnhandledException += (s, e) => SaveToDisk();
            }
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
            _diskManager.SaveTodisk(this, _diskLocation);
        }

        public void LoadFromDisk()
        {
            _diskManager.LoadFromDisk(this, _diskLocation);
        }

        public void ClearDb()
        {
            _tables.Clear();
        }

        public void SetDiskLocation(string location)
        {
            _diskLocation = location;
        }
    }
}
