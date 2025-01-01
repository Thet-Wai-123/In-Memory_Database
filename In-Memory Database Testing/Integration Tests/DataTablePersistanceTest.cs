using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using In_Memory_Database;
using In_Memory_Database.Classes;
using Microsoft.Extensions.DependencyInjection;

namespace In_Memory_Database_Testing
{
    public class DataTablePersistanceTest
    {
        [Fact]
        public void SavingAndLoading_ExpectSameInfosAndSearchManagerGetsReInjectedUponLoading()
        {
            ServiceCollection sc = new();
            Startup startup = new();
            startup.ConfigureServices(sc);
            ServiceProvider serviceProvider = sc.BuildServiceProvider();

            //Arrange
            DataTable table =
                new(
                    "AgeTable",
                    ["Age"],
                    [typeof(int)],
                    serviceProvider.GetRequiredService<ISearchManager>()
                );
            var row1 = new DataRow { 20 };
            var row2 = new DataRow { 21 };
            var row3 = new DataRow { 22 };
            var row4 = new DataRow { 23 };
            table.AddRow(row1);
            table.AddRow(row2);
            table.AddRow(row3);
            table.AddRow(row4);

            //Act
            string path = (@"temp\");
            table.SaveToDisk(path);
            IDataTable loadedTable = FileManager.LoadFromDisk(path)[0];
            //test to make sure the SearchManager also get injected upon deserializing
            var res = loadedTable.Get(new SearchConditions("Age", "==", 21));

            //Assert
            Assert.True(File.Exists(path + "AgeTable.json"));
            Assert.Equal(table.Name, loadedTable.Name);
            Assert.Equal(table.Rows, loadedTable.Rows);
            Assert.Equal(row2, res[0]);

            Directory.Delete(path, true);
        }
    }
}
