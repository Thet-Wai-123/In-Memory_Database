using In_Memory_Database.Classes.Data;

namespace In_Memory_Database.Classes.Dependencies.Managers
{
    public interface IFileManager
    {
        public void SaveTodisk(Database db, string dir);
        public Dictionary<String, DataTable> LoadFromDisk(string dir);
    }
}
