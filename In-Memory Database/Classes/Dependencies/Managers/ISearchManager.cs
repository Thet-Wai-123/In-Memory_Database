using In_Memory_Database.Classes.Data;
using System.Collections.ObjectModel;

namespace In_Memory_Database.Classes.Dependencies.Managers
{
    public interface ISearchManager
    {
        public List<DataRow> Search(
            ReadOnlyCollection<string> columnNames,
            List<DataRow> rows,
            SearchConditions conditions,
            ReadOnlyDictionary<string, IndexTable> indexTables,
            bool useIndex = true
        );
    }
}
