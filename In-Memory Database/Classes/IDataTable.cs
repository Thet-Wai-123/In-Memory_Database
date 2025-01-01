using System.Collections.ObjectModel;

namespace In_Memory_Database.Classes
{
    public interface IDataTable
    {
        string Name { get; set; }
        ReadOnlyCollection<Type> ColumnTypes { get; }
        ReadOnlyCollection<string> ColumnNames { get; }
        ReadOnlyCollection<DataRow> Rows { get; }
        ReadOnlyDictionary<string, IndexTable> IndexTables { get; }
        string Size { get; }
        int Width { get; }
        int Height { get; }

        public void AddColumn(string name, Type type);
        public void RemoveColumn(string name);
        public void AddRow(DataRow row);
        public void RemoveRow(SearchConditions conditions);

        public List<DataRow> Get(SearchConditions conditions, bool useIndex = true);

        public void ClearTable();

        public void SaveToDisk(string dir) => FileManager.SaveToDisk(this, dir);

        public void LoadFromDisk(string dir);

        public void StartTransaction();

        public void CommitTransaction();

        public void CreateIndex(string targetColumn);
        public void RemoveIndex();
    }
}
