using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using static System.Runtime.InteropServices.JavaScript.JSType;

//dotnet build
//dotnet run -c Release
namespace In_Memory_Database_Benchmark
{
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 1)]
    public class IndexBenchMark
    {
        static readonly int rowCount = 1000000;

        Random rnd = new();
        static List<string> columnNames = ["Name", "Age", "Weight", "IsAlive"];
        static List<Type> types = [typeof(string), typeof(int), typeof(double), typeof(bool)];

        SearchManager searchManager = new();

        DataTable tempTable = new("Temp", columnNames, types, new SearchManager());

        DataRow? rowToFind;

        [GlobalSetup(Target = nameof(CreatingIndex))]
        public void SetupWithoutIndex()
        {
            FillInTableWithRandomRows(tempTable);
            rowToFind = tempTable.Rows[rnd.Next(1, rowCount)];
        }

        [GlobalSetup(Targets = [nameof(SearchStringWithoutIndex), nameof(SearchStringWithIndex)])]
        public void SetupWithIndex()
        {
            FillInTableWithRandomRows(tempTable);
            tempTable.CreateIndex("Name");
            rowToFind = tempTable.Rows[rnd.Next(1, rowCount)];
        }

        [Benchmark]
        public void CreatingIndex()
        {
            tempTable.CreateIndex("Age");
        }

        [Benchmark]
        public ReadOnlyCollection<DataRow> SearchStringWithoutIndex()
        {
            var condition = new SearchConditions("Name", "==", rowToFind[0]);
            return searchManager.Search(
                tempTable.ColumnNames,
                tempTable.Rows,
                condition,
                tempTable.IndexTables,
                false
            );
        }

        [Benchmark]
        public ReadOnlyCollection<DataRow> SearchStringWithIndex()
        {
            var condition = new SearchConditions("Name", "==", rowToFind[0]);
            return searchManager.Search(
                tempTable.ColumnNames,
                tempTable.Rows,
                condition,
                tempTable.IndexTables,
                true
            );
        }

        private void FillInTableWithRandomRows(IDataTable table)
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
            var row = new DataRow() { param1, param2, param3, param4 };
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
