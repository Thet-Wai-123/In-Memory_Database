namespace In_Memory_Database.Classes.Dependencies.Managers
{
    public class SearchConditions
    {
        public string ColumnName { get; }
        public string Op { get; }
        public object Value { get; }

        public SearchConditions(string columnName, string op, object value)
        {
            ColumnName = columnName;
            Op = op;
            Value = value;
        }
    }
}
