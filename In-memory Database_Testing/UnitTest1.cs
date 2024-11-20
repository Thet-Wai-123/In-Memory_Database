using In_Memory_Database.Classes;

namespace In_memory_Database_Testing
{
    public class UnitTest1
    {
        //Add some wrong cases to test exception handling!!
        [Fact]
        public void CreateTableWithRowsAndColumns_ExpectTableSizeToChange()
        {
            DataTable table = new("Student");
            Assert.Equal("Student", table.TableName);

            table.AddColumn<string>("Name");
            table.AddColumn<int>("age");
            Assert.Equal("0x2", table.Size);

            table.AddRow(new List<dynamic> { "John", 20 });
            table.AddRow(new List<dynamic> { "Jane", 21 });
            Assert.Equal("2x2", table.Size);
        }

        [Fact]
        public void TableBasicOperations() { }
    }
}
