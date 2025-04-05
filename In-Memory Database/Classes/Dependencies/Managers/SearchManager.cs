using System.Collections.Generic;
using System.Collections.ObjectModel;
using In_Memory_Database.Classes.Data;

namespace In_Memory_Database.Classes.Dependencies.Managers
{
    public class SearchManager : ISearchManager
    {
        //A general search and will automatically use search by index if it exists, otherwise sequential search
        public List<DataRow> Search(
            ReadOnlyCollection<string> columnNames,
            ReadOnlyCollection<DataRow> rows,
            SearchConditions conditions,
            ReadOnlyDictionary<string, IndexTable> indexTables,
            bool useIndex = true
        )
        {
            ReadOnlyCollection<string> _columnNames = columnNames;
            ReadOnlyCollection<DataRow> _rows = rows;
            ReadOnlyDictionary<string, IndexTable> _indexTables = indexTables;

            //First making sure that the column exists
            int targetColumnIndex = -1;
            for (int i = 0; i < _columnNames.Count; i++)
            {
                if (_columnNames[i] == conditions.ColumnName)
                {
                    targetColumnIndex = i;
                    break;
                }
            }
            if (targetColumnIndex == -1)
            {
                return [];
            }

            //It does exist, so search
            List<DataRow> matchingRows;

            //Checking if an index exists to use
            if (useIndex && _indexTables.ContainsKey(conditions.ColumnName))
            {
                var indexTable = _indexTables[conditions.ColumnName];
                matchingRows = IndexSearch(indexTable, conditions);
            }
            else
            {
                matchingRows = SequentialSearch(_rows, targetColumnIndex, conditions);
            }
            return matchingRows;
        }

        private List<DataRow> IndexSearch(IndexTable indexTable, SearchConditions conditions)
        {
            dynamic keyValue = conditions.Value;
            string op = conditions.Op;
            return indexTable.Search(keyValue, op);
        }

        private List<DataRow> SequentialSearch(
            ReadOnlyCollection<DataRow> rows,
            int targetColumnIndex,
            SearchConditions conditions
        )
        {
            List<DataRow> matchingRows = [];
            foreach (DataRow row in rows)
            {
                if (row.Count > targetColumnIndex)
                {
                    dynamic valueToCheck = row[targetColumnIndex];
                    if (CheckConditionOneByOne(valueToCheck, conditions.Op, conditions.Value))
                    {
                        matchingRows.Add(row);
                    }
                }
            }
            return matchingRows;

            bool CheckConditionOneByOne(dynamic value, string op, dynamic targetValue)
            {
                switch (op)
                {
                    case "==":
                        return value == targetValue;
                    case ">=":
                        return value >= targetValue;
                    case "<=":
                        return value <= targetValue;
                    default:
                        throw new ArgumentException($"Unsupported operator: {op}");
                }
            }
        }
    }
}
