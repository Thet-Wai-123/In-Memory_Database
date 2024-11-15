public class FileManager
{
    public static void SaveToDisk(Dictionary<Guid, List<dynamic>> rows, string path)
    {
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
