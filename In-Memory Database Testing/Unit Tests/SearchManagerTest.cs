using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using Moq;
using System.Collections.ObjectModel;

namespace In_Memory_Database_Testing
{
    public class SearchManagerTest
    {
        [Fact]
        public void TableFindRow_ExpectToReturnCorrectRow()
        {
            //Arrange
            var searchManager = new SearchManager();
            var rows = new List<DataRow>();
            var mockTable = new Mock<IDataTable>();
            mockTable.SetupGet(x => x.Rows).Returns(rows.AsReadOnly());
            mockTable.SetupGet(x => x.ColumnNames).Returns(new ReadOnlyCollection<string>(["Age"]));
            mockTable
                .SetupGet(x => x.IndexTables)
                .Returns(
                    new ReadOnlyDictionary<string, IndexTable>(new Dictionary<string, IndexTable>())
                );
            mockTable
                .Setup(x => x.AddRow(It.IsAny<DataRow>()))
                .Callback<DataRow>(row => rows.Add(row));

            var condition = new SearchConditions("Age", "==", 21);

            var row1 = new DataRow { 20 };
            var row2 = new DataRow { 21 };
            var row3 = new DataRow { 22 };
            var row4 = new DataRow { 23 };

            mockTable.Object.AddRow(row1);
            mockTable.Object.AddRow(row2);
            mockTable.Object.AddRow(row3);
            mockTable.Object.AddRow(row4);

            //Act
            List<DataRow> Rows = mockTable.Object.Rows.ToList();
            var searchResult = searchManager.Search(
                mockTable.Object.ColumnNames,
                Rows,
                condition,
                mockTable.Object.IndexTables
            );

            //Assert
            Assert.Equal(
                [
                    [21]
                ],
                searchResult
            );
        }
    }
}
