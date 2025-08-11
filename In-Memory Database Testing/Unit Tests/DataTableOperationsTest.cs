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
        public async void TableAddAndDelete_ExpectOneRowLeft()
        {
            //Arrange
            var table = new DataTable("AgeTable", ["Age"], [typeof(int)], new SearchManager());
            var row1 = new DataRow { 1 };
            var row2 = new DataRow { 2 };
            var row3 = new DataRow { 3 };
            var row4 = new DataRow { 4 };

            //Act
            await table.AddRow(row1);
            await table.AddRow(row2);
            await table.AddRow(row3);
            await table.AddRow(row4);
            await table.RemoveRow(new SearchConditions("Age", "==", 2));

            //Assert
            Assert.Equal("1x3", table.Size);
            //It should re-organize the rows, so now the second element is the third row.
            Assert.Equal(table.Rows[1], row3);
        }

        [Fact]
        public async void TableSearchRow_ExpectCorrectRowReturned()
        {
            //Arrange
            var table = new DataTable("AgeTable", ["Age"], [typeof(int)], new SearchManager());
            var row1 = new DataRow { 1 };
            var row2 = new DataRow { 2 };
            var row3 = new DataRow { 3 };
            var row4 = new DataRow { 4 };
            await table.AddRow(row1);
            await table.AddRow(row2);
            await table.AddRow(row3);
            await table.AddRow(row4);

            //Act
            var searchResult = await table.Search(new SearchConditions("Age", "==", 2));

            //Assert
            Assert.Equal(new List<DataRow> { row2 }, searchResult);
        }

        [Fact]
        public async void TableUpdateRows_ExpectUpdatedChanges()
        {
            //Arrange
            var table = new DataTable("AgeTable", ["Age"], [typeof(int)], new SearchManager());
            var row1 = new DataRow { 1 };
            var row2 = new DataRow { 2 };
            var row3 = new DataRow { 3 };
            await table.AddRow(row1);
            await table.AddRow(row2);
            await table.AddRow(row3);

            //Act
            await table.UpdateRow(new SearchConditions("Age", "==", 1), "Age", 4);
            await table.UpdateRow(new SearchConditions("Age", "==", 2), "Age", 5);

            //Assert
            //Now the only values for age in tables should be 3,4,5 but they're not sorted in order
            Assert.Equal(3, table.Height);

            var expectedValues = new List<int> { 3, 4, 5 };
            foreach (var row in table.Rows)
            {
                Assert.Contains(expectedValues, value => value == row[0]);
            }
        }

        [Fact]
        public async void TableAddAndRemoveColumn_ExpectTheRowsToMatch()
        {
            //Arrange
            var table = new DataTable("AgeTable", ["Age"], [typeof(int)], new SearchManager());
            var row1 = new DataRow { 1 };
            var row2 = new DataRow { 2 };
            var row3 = new DataRow { 3 };
            var row4 = new DataRow { 4 };
            await table.AddRow(row1);
            await table.AddRow(row2);
            await table.AddRow(row3);
            await table.AddRow(row4);

            //Act & Assert
            await table.AddColumn("Score", typeof(int));
            Assert.Equal(2, table.Width);
            await table.RemoveColumn("Age");
            Assert.Equal(1, table.Width);
        }

        [Fact]
        public async void VacuumTest_ExpectOldRowsToGetDeleted()
        {
            //Arrange
            var table = new DataTable("AgeTable", ["Age"], [typeof(int)], new SearchManager());
            var row1 = new DataRow { 1 };
            await table.AddRow(row1);
            await table.CreateIndex("Age");

            //Act
            await table.UpdateRow(new SearchConditions("Age", "==", 1), "Age", 2);
            await table.UpdateRow(new SearchConditions("Age", "==", 2), "Age", 3);
            var tableLengthBefore = table.getRowsLengthIncludeHidden();
            var indexLengthBefore = table.getIndexTableLength("Age");

            await table.VacuumInactiveRows();
            var tableLengthAfter = table.getRowsLengthIncludeHidden();
            var indexLengthAfter = table.getIndexTableLength("Age");

            Assert.Equal(3, tableLengthBefore);
            Assert.Equal(3, indexLengthBefore);
            //Now the previous 2 versions has been dropped
            Assert.Equal(1, tableLengthAfter);
            Assert.Equal(1, indexLengthAfter);
        }
    }
}
