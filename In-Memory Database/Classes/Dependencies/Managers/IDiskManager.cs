using In_Memory_Database.Classes.Data;

namespace In_Memory_Database.Classes.Dependencies.Managers
{
    public interface IDiskManager
    {
        public void SaveTodisk(Database db, string dir);
        public void LoadFromDisk(Database db, string dir);
    }
}
