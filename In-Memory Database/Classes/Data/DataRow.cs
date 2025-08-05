namespace In_Memory_Database.Classes.Data
{
    public class DataRow :List<dynamic>
    {
        public DataRow()
        {
        }

        public DataRow(DataRow copy)
            : base(copy) { }

        private Guid _tid;
        public Guid Tid
        {
            get => _tid;
            init => _tid = new Guid();
        }
    }
}
