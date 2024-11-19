using Web_API_Database.Classes;

public class FileManager
{
    public static void SaveToDisk(TableSnapShot table, string path)
    {
        Dictionary<Guid, List<dynamic>> rows = table.Rows;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        using (StreamWriter sw = File.CreateText(path))
        {
            foreach (var row in rows)
            {
                string line = row.Key + "|";
                foreach (var value in row.Value)
                {
                    line += value + "|";
                }
                sw.WriteLine(line);
            }
        }
    }

    public void ReadFromDisk(string path) { }
}
