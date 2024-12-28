using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using In_Memory_Database.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

//dotnet build
//dotnet run -c Release
namespace In_Memory_Database_Benchmark
{
    [SimpleJob(
        RunStrategy.ColdStart /*, iterationCount: 1*/
    )]
    public class IndexBenchMark
    {
        static readonly int rowCount = 1000000;

        Random rnd = new();
        static List<string> columnNames = new() { "Name", "Age", "Weight", "IsAlive" };
        static List<Type> types =
            new() { typeof(string), typeof(int), typeof(double), typeof(bool) };

        DataTable nonIndexedTable = new("NonIndexed", columnNames, types);
        DataTable indexedTable = new("Indexed", columnNames, types);
        DataTable tempTable = new("Temp", columnNames, types);

        DataRow rowToFind;

        [GlobalSetup(Target = nameof(CreatingIndex))]
        public void SetupA()
        {
            FillInTableWithRandomRows(tempTable);
            rowToFind = tempTable.Rows[rnd.Next(1, rowCount)];
        }

        [GlobalSetup(
            Targets = new[] { nameof(SearchStringWithoutIndex), nameof(SearchStringWithIndex) }
        )]
        public void SetupB()
        {
            FillInTableWithRandomRows(indexedTable);
            FillInTableWithRandomRows(nonIndexedTable);
            indexedTable.CreateIndex("Name");
            rowToFind = nonIndexedTable.Rows[rnd.Next(1, rowCount)];
        }

        [Benchmark]
        public void CreatingIndex()
        {
            tempTable.CreateIndex("Age");
        }

        [Benchmark]
        public List<DataRow> SearchStringWithoutIndex()
        {
            return nonIndexedTable.Find(new SearchConditions("Name", "==", rowToFind[0]));
        }

        [Benchmark]
        public List<DataRow> SearchStringWithIndex()
        {
            return indexedTable.Find(new SearchConditions("Name", "==", rowToFind[0]));
        }

        private void FillInTableWithRandomRows(DataTable table)
        {
            for (int i = 0; i < rowCount; i++)
            {
                var randomRow = GenerateRandomRow();
                table.AddRow(randomRow);
            }
        }

        private DataRow GenerateRandomRow()
        {
            var param1 = RandomString(10);
            var param2 = rnd.Next(1, 100);
            var param3 = Math.Round(rnd.NextDouble() * 100, 2);
            var param4 = rnd.Next(0, 2) == 1;
            DataRow row = [param1, param2, param3, param4];
            return row;
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(
                Enumerable.Repeat(chars, length).Select(s => s[rnd.Next(s.Length)]).ToArray()
            );
        }
    }
}
