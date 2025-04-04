using In_Memory_Database.Classes.Data;
using Microsoft.CSharp.RuntimeBinder;
using Xunit.Abstractions;

namespace In_Memory_Database_Testing
{
    public class IndexTableTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public IndexTableTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void CreateIndexWithWrongDataTypeInRows_ExpectToThrowException()
        {
            //Arrange
            DataRow row1 = new() { "Person1", 1 };
            DataRow row2 = new() { "Person2", 2 };
            DataRow row3 = new() { "Person3", "A String" };
            List<DataRow> rows = new() { row1, row2, row3 };

            //Act
            Action act = () => new IndexTable<int>(1, rows);

            //Assert
            Assert.Throws<RuntimeBinderException>(act);
        }

        [Fact]
        public void SearchFromIndex()
        {
            //Arrange
            DataRow row1 = new() { "Person1", 1 };
            DataRow row2 = new() { "Person2", 2 };
            DataRow row3 = new() { "Person3", 3 };
            List<DataRow> rows = new() { row1, row2, row3 };
            IndexTable<int> indexTable = new(1, rows);

            //Act
            List<DataRow> result = indexTable.Search(2, ">=");
            _testOutputHelper.WriteLine(result.ToString());

            //Assert
            Assert.Equal([row2, row3], result);
        }

        [Fact]
        public void DeletingRowsWithIndexSearch()
        {
            //Arrange
            const int columnIndex = 1;
            DataRow row1 = new() { "Person1", 1 };
            DataRow row2 = new() { "Person2", 2 };
            DataRow row3 = new() { "Person3", 3 };
            List<DataRow> rows = new() { row1, row2, row3 };
            IndexTable<int> indexTable = new(columnIndex, rows);

            //Act
            indexTable.Delete(columnIndex, row2);
            List<DataRow> result = indexTable.Search(2, ">=");
            //_testOutputHelper.WriteLine(result.ToString());

            //Assert
            Assert.Equal([row3], result);
        }

        [Fact]
        public void HandlingDuplicateValues()
        {
            //Arrange
            DataRow row1 = new() { "Person1", 1 };
            DataRow row2 = new() { "Person1Duplicate", 1 };
            DataRow row3 = new() { "Person3", 3 };
            List<DataRow> rows = new() { row1, row2, row3 };
            IndexTable<int> indexTable = new(1, rows);

            //Act
            List<DataRow> result = indexTable.Search(1, "==");
            //_testOutputHelper.WriteLine(result.ToString());

            //Assert
            Assert.Equal([row1, row2], result, ScrambledEquals);
        }

        //Comparing two lists without caring about the order
        private static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
            where T : notnull
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
    }
}
