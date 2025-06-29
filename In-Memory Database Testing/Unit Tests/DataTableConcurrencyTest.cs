using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace In_Memory_Database_Testing.Unit_Tests
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


        //Currently, the list cannot handle concurrency, only getting around 234 rows instead of 300
        [Fact]
        public void AddingMultipleRowsConcurrently_ExpectCorrectRowsCount()
        {
            //Arrange
            var table = new DataTable("Test", ["column1"], [typeof(int)], new SearchManager());

            //Act
            Task taskA = Task.Run(() => Add100Rows(table));
            Task taskB = Task.Run(() => Add100Rows(table));
            Task taskC = Task.Run(() => Add100Rows(table));
            Task.WaitAll([taskA, taskB, taskC]);

            //Assert
            Assert.Equal(300, table.Height);
        }

        private void Add100Rows(DataTable table)
        {
            for (int i = 0; i < 100; i++)
            {
                table.AddRow(new DataRow { 1 });
            }
        }
    }
}
