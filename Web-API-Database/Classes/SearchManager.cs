using In_Memory_Database.Classes;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Web_API_Database.Classes
{
    public static class SearchManager
    {
        public static List<DataRow> Get(
            DataTable table,
            SearchConditions conditions
        )
        {
            ReadOnlyCollection<string> columnNames = table.ColumnNames;
            ReadOnlyCollection<DataRow> rows = table.Rows;

            //First making sure that the column exists
            int targetColumnIndex = -1;
            for (int i = 0; i < columnNames.Count; i++)
            {
                if (columnNames[i] == conditions.ColumnName)
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
            if (table.IndexTables.ContainsKey(conditions.ColumnName))
            {
                var indexTable = table.IndexTables[conditions.ColumnName];
                matchingRows = IndexSearch(indexTable, conditions.Value, conditions.Op);
            }
            else
            {
                matchingRows = SequentialSearch(rows, targetColumnIndex, conditions);
            }
            return matchingRows;
        }

        private static List<DataRow> IndexSearch(IndexTable indexTable, object keyValue, string op)
        {
            return indexTable.Search(keyValue, op);
        }

        private static List<DataRow> SequentialSearch(
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
