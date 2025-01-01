using In_Memory_Database.Classes;
using Newtonsoft.Json;

public class FileManager
{
    public static void SaveToDisk(IDataTable table, string dir)
    {
        string jsonString = JsonConvert.SerializeObject(table);

        Directory.CreateDirectory(Path.GetDirectoryName(dir));
        string path = $"{dir}{table.Name}.json";

        File.WriteAllText(path, jsonString);
    }

    public static List<IDataTable> LoadFromDisk(string dir)
    {
        string[] tablesToBeGenerated = Directory.GetFiles(dir);
        List<IDataTable> tablesToReturn = [];

        foreach (string tablePath in tablesToBeGenerated)
        {
            using (StreamReader sr = File.OpenText(tablePath))
            {
                string infoJson = sr.ReadToEnd();
                tablesToReturn.Add(JsonConvert.DeserializeObject<DataTable>(infoJson));
            }
        }
        return tablesToReturn;
    }
}
