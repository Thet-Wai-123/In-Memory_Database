using In_Memory_Database.Classes;
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
            DataTable table = new("Student");
            table.AddColumn("Name", typeof(string));
            table.AddColumn("Age", typeof(int));
            table.AddRow(new DataRow { "A", 20 });
            table.AddRow(new DataRow { "B", 21 });
            table.AddRow(new DataRow { "C", 22 });
            table.AddRow(new DataRow { "D", 20 });

            //Act
            table.RemoveRow(new SearchConditions("Age", "<=", 21));

            //Assert
            Assert.Equal("1x2", table.Size);
        }

        [Fact]
        public void TableFindRow_ExpectToReturnCorrectRow()
        {
            //Arrange
            DataTable table = new("Student");
            table.AddColumn("Name", typeof(string));
            table.AddColumn("Age", typeof(int));
            table.AddRow(new DataRow { "A", 20 });
            table.AddRow(new DataRow { "B", 21 });
            table.AddRow(new DataRow { "C", 22 });
            table.AddRow(new DataRow { "D", 20 });

            //Act
            var emptyRes = table.Find(new SearchConditions("Name", "==", "E"));
            var singleRowRes = table.Find(new SearchConditions("Name", "==", "A"));
            var doubleRowRes = table.Find(new SearchConditions("Age", "==", 20));

            //Assert
            Assert.Empty(emptyRes);
            Assert.Equal(
                new List<DataRow>
                {
                    new() { "A", 20 }
                },
                singleRowRes
            );
            Assert.Equal(
                new List<DataRow>
                {
                    new() { "A", 20 },
                    new() { "D", 20 }
                },
                doubleRowRes
            );
        }

        [Fact]
        public void SavingAndLoading_ExpectEqualInfos()
        {
            //Arrange
            DataTable table = new("Student");
            table.AddColumn("Name", typeof(string));
            table.AddColumn("Age", typeof(int));
            table.AddRow(new DataRow { "A", 20 });
            table.AddRow(new DataRow { "B", 21 });
            table.AddRow(new DataRow { "C", 22 });
            table.AddRow(new DataRow { "D", 20 });

            //Act
            string path = (@"Web-API-Database\In-memory DatabaseTesting\temp\");
            table.SaveToDisk(path);
            DataTable loadedTable = FileManager.LoadFromDisk(path)[0];

            //Assert
            Assert.True(File.Exists(path + "Student.json"));
            Assert.Equal(table.Name, loadedTable.Name);
            Assert.Equal(table.Rows, loadedTable.Rows);
        }
    }
}
