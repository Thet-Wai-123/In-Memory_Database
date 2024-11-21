using In_Memory_Database.Classes;
using Web_API_Database.Classes;

namespace In_memory_Database_Testing
{
    public class DataTableOperationsTest
    {
        [Fact]
        public void TableDeleteRow_ExpectOneRowLeft()
        {
            //Arrange
            DataTable table = new("Student");
            table.AddColumn<string>("Name");
            table.AddColumn<int>("Age");
            table.AddRow(new DataRow { "A", 20 });
            table.AddRow(new DataRow { "B", 21 });
            table.AddRow(new DataRow { "C", 22 });
            table.AddRow(new DataRow { "D", 20 });

            //Act
            table.RemoveRow(new SearchConditions("Age", "<", 22));

            //Assert
            Assert.Equal("1x2", table.Size);
        }

        [Fact]
        public void TableFindRow_ExpectToReturnRow()
        {
            //Arrange
            DataTable table = new("Student");
            table.AddColumn<string>("Name");
            table.AddColumn<int>("Age");
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
    }
}
