using System.Collections.ObjectModel;
using In_Memory_Database.Classes.Dependencies.Managers;

namespace In_Memory_Database.Classes.Data
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
        public void UpdateRow(SearchConditions searchConditions, string column, object newValue);
        public void RemoveRow(SearchConditions searchConditions);
        public ReadOnlyCollection<DataRow> Search(SearchConditions conditions);
        public void ClearTable();
        public void CreateIndex(string targetColumn);
        public void DeleteIndex(string targetColumn);
    }
}
