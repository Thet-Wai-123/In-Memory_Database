using In_Memory_Database.Classes.Data;
using Newtonsoft.Json;

namespace In_Memory_Database.Classes.Dependencies.Managers
{
    public class FileManager : IFileManager
    {
        public void SaveTodisk(Database db, string dir)
        {
            foreach (var table in db.Tables.Values)
            {
                string jsonString = JsonConvert.SerializeObject(table);

                Directory.CreateDirectory(Path.GetDirectoryName(dir));
                string path = $"{dir}{table.Name}.json";

                File.WriteAllText(path, jsonString);
            }
        }

        public Dictionary<String, DataTable> LoadFromDisk(string dir)
        {
            string[] tablesFoundInDir = Directory.GetFiles(dir);
            String[] tableNames = tablesFoundInDir
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
            Dictionary<String, DataTable> tablesGenerated = [];

            int i = 0;
            foreach (string tablePath in tablesFoundInDir)
            {
                using (StreamReader sr = File.OpenText(tablePath))
                {
                    string infoJson = sr.ReadToEnd();
                    var deserializedTable = JsonConvert.DeserializeObject<DataTable>(infoJson);
                    tablesGenerated.Add(tableNames[i], deserializedTable);
                    i++;
                }
            }
            return tablesGenerated;
        }
    }
}
