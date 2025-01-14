using System;
using Castle.Components.DictionaryAdapter;
using In_Memory_Database.Classes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace In_Memory_Database_Testing
{
    public class DataTableOperationsTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DataTableOperationsTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TableAddAndDeleteRow_ExpectOneRowLeft()
        {
            //Arrange
            var searchManagerMock = new Mock<ISearchManager>();
            var table = new DataTable("AgeTable", ["Age"], [typeof(int)]);
            var row1 = new DataRow { 20 };
            var row2 = new DataRow { 21 };
            var row3 = new DataRow { 22 };
            var row4 = new DataRow { 23 };
            table.AddRow(row1);
            table.AddRow(row2);
            table.AddRow(row3);
            table.AddRow(row4);

            SearchConditions conditions = new("Age", "<=", 21);
            searchManagerMock
                .Setup(x =>
                    x.Search(table.ColumnNames, table.Rows, conditions, table.IndexTables, true)
                )
                .Returns([row1, row2]);

            //Act
            var rowToRemove = searchManagerMock.Object.Search(
                table.ColumnNames,
                table.Rows,
                conditions,
                table.IndexTables,
                true
            );
            table.RemoveRow(rowToRemove);

            //Assert
            Assert.Equal("1x2", table.Size);
            Assert.Equal(table.Rows[0], row3);
            Assert.Equal(table.Rows[1], row4);
        }
    }
}
