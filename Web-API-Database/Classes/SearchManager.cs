using System.Collections.Generic;
using In_Memory_Database.Classes;

namespace Web_API_Database.Classes
{
    //my plan is to add more stuffs here such as a query planner
    public static class SearchManager
    {
        public static List<Guid> SearchTableForIdUsingConditions(
            DataTable table,
            SearchConditions conditions
        )
        {
            List<string> columnNames = table.ColumnNames;
            Dictionary<Guid, DataRow> rows = table.Rows;

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

            List<Guid> matchingIds = new();

            // Iterate through the rows and check the condition
            foreach (var row in rows)
            {
                Guid id = row.Key;
                List<dynamic> values = row.Value;

                if (values.Count > targetColumnIndex)
                {
                    dynamic valueToCheck = values[targetColumnIndex];
                    if (CheckCondition(valueToCheck, conditions.Op, conditions.Value))
                    {
                        matchingIds.Add(id);
                    }
                }
            }
            return matchingIds;
        }

        private static bool CheckCondition(dynamic value, string op, dynamic targetValue)
        {
            switch (op)
            {
                case "==":
                    return value == targetValue;
                case "!=":
                    return value != targetValue;
                case ">":
                    return value > targetValue;
                case "<":
                    return value < targetValue;
                default:
                    throw new ArgumentException($"Unsupported operator: {op}");
            }
        }
    }
}
