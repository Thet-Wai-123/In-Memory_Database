using System.Collections.ObjectModel;
using In_Memory_Database.Classes.Data;

namespace In_Memory_Database.Classes.Dependencies.Managers
{
    public interface ISearchManager
    {
        public List<DataRow> Search(
            ReadOnlyCollection<string> columnNames,
            ReadOnlyCollection<DataRow> rows,
            SearchConditions conditions,
            ReadOnlyDictionary<string, IndexTable> indexTables,
            bool useIndex = true
        );
    }
}
