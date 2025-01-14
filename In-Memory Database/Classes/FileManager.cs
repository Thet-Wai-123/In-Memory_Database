using In_Memory_Database.Classes;
using Newtonsoft.Json;

public class FileManager : IFileManager
{
    public void SaveTodisk(Database db, string dir)
    {
        foreach (var table in db.tables)
        {
            string jsonString = JsonConvert.SerializeObject(table);

            Directory.CreateDirectory(Path.GetDirectoryName(dir));
            string path = $"{dir}{table.Name}.json";

            File.WriteAllText(path, jsonString);
        }
    }

    public List<DataTable> LoadFromDisk(string dir)
    {
        string[] tablesFoundInDir = Directory.GetFiles(dir);
        List<DataTable> tablesGenerated = [];

        foreach (string tablePath in tablesFoundInDir)
        {
            using (StreamReader sr = File.OpenText(tablePath))
            {
                string infoJson = sr.ReadToEnd();
                var deserializedTable = (JsonConvert.DeserializeObject<DataTable>(infoJson));
                tablesGenerated.Add(deserializedTable);
            }
        }
        return tablesGenerated;
    }
}
