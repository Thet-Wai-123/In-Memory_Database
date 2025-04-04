using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
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
        public void TableAddAndDelete_ExpectOneRowLeft()
        {
            //Arrange
            var table = new DataTable("AgeTable", ["Age"], [typeof(int)], new SearchManager());
            var row1 = new DataRow { 1 };
            var row2 = new DataRow { 2 };
            var row3 = new DataRow { 3 };
            var row4 = new DataRow { 4 };

            //Act
            table.AddRow(row1);
            table.AddRow(row2);
            table.AddRow(row3);
            table.AddRow(row4);
            var rowToRemove = new List<DataRow> { row2 };
            table.RemoveRow(rowToRemove);

            //Assert
            Assert.Equal("1x3", table.Size);
            //It should re-organize the rows, so now the second element is the third row.
            Assert.Equal(table.Rows[1], row3);
        }

        [Fact]
        public void TableSearchRow_ExpectCorrectRowReturned()
        {
            //Arrange
            var table = new DataTable("AgeTable", ["Age"], [typeof(int)], new SearchManager());
            var row1 = new DataRow { 1 };
            var row2 = new DataRow { 2 };
            var row3 = new DataRow { 3 };
            var row4 = new DataRow { 4 };
            table.AddRow(row1);
            table.AddRow(row2);
            table.AddRow(row3);
            table.AddRow(row4);

            //Act
            var searchResult = table.Search(new SearchConditions("Age", "==", 2));

            //Assert
            Assert.Equal(new List<DataRow> { row2 }, searchResult);
        }
    }
}
