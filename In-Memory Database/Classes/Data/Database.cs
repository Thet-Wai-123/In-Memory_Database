using In_Memory_Database.Classes.Dependencies.Managers;
using System.Collections.ObjectModel;

namespace In_Memory_Database.Classes.Data
{
    public class Database
    {
        private Dictionary<string, DataTable> _tables = new();
        public ReadOnlyDictionary<string, DataTable> Tables
        {
            get
            {
                return _tables.AsReadOnly();
            }
        }
        private readonly ISearchManager _searchManager;
        private readonly IDiskManager _diskManager;

        private string _diskLocation;

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
            var generatedTable = new DataTable(tableName, columnNames, columnTypes, _searchManager);
            _tables.Add(tableName, generatedTable);

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    _tables[tableName].AddRow(row);
                }
            }
        }

        public void CopyTables(Dictionary<string, DataTable> tables)
        {
            _tables = tables;
            foreach (var table in _tables.Values)
            {
                table.SetSearchManager(_searchManager);
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

        public void Begin()
        {
            TransactionManager.Begin();
        }

        public void Commit()
        {
            TransactionManager.Commit();
        }

        public void Abort()
        {
            TransactionManager.RollBack();
        }
    }
}
