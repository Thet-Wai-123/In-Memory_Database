using System.Collections.ObjectModel;

namespace In_Memory_Database.Classes
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
