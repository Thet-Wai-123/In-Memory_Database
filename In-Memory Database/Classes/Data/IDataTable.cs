using In_Memory_Database.Classes.Dependencies.Managers;
using System.Collections.ObjectModel;

namespace In_Memory_Database.Classes.Data
{
    public interface IDataTable
    {
        string Name
        {
            get; set;
        }
        ReadOnlyCollection<Type> ColumnTypes
        {
            get;
        }
        ReadOnlyCollection<string> ColumnNames
        {
            get;
        }
        ReadOnlyCollection<DataRow> Rows
        {
            get;
        }
        ReadOnlyDictionary<string, IndexTable> IndexTables
        {
            get;
        }
        string Size
        {
            get;
        }
        int Width
        {
            get;
        }
        int Height
        {
            get;
        }

        public Task AddColumn(string name, Type type, object defaultValue);
        public Task RemoveColumn(string name);
        public Task AddRow(DataRow row);
        public Task UpdateRow(SearchConditions searchConditions, string column, object newValue);
        public Task RemoveRow(SearchConditions searchConditions);
        public Task<ReadOnlyCollection<DataRow>> Search(SearchConditions conditions);
        public Task ClearTable();
        public Task CreateIndex(string targetColumn);
        public Task DeleteIndex(string targetColumn);
    }
}
