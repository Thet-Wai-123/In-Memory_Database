namespace In_Memory_Database.Classes
{
    public interface IFileManager
    {
        public void SaveTodisk(Database db, string dir);
        public List<DataTable> LoadFromDisk(string dir);
    }
}
