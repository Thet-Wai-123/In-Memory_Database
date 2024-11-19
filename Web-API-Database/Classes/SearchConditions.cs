namespace Web_API_Database.Classes
{
    public class SearchConditions
    {
        public String ColumnName { get; }
        public String Op { get; }
        public Object Value { get; }

        public SearchConditions(String columnName, String op, Object value)
        {
            ColumnName = columnName;
            Op = op;
            Value = value;
        }
    }
}
