using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace In_Memory_Database_Testing
{
    public class DataTableConcurrencyTest
    {
        //For debugging, delete later
        private readonly ITestOutputHelper _testOutputHelper;

        public DataTableConcurrencyTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        //end of debugging

        [Fact]
        public async void AddingMultipleRowsConcurrently_ExpectCorrectRowsCount()
        {
            //Arrange
            var table = new DataTable("Test", ["column1"], [typeof(int)], new SearchManager());

            //Act
            Task taskA = Add100Rows(table);
            Task taskB = Add100Rows(table);
            Task taskC = Add100Rows(table);
            await Task.WhenAll(taskA, taskB, taskC);

            //Assert
            Assert.Equal(300, table.Height);
        }

        private async Task Add100Rows(DataTable table)
        {
            for (int i = 0; i < 100; i++)
            {
                await table.AddRow(new DataRow { 1 });
            }
        }
    }
}
