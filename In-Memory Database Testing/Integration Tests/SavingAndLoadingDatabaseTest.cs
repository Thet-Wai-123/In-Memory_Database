using System.Reflection;
using System.Text.Json;
using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using Xunit.Abstractions;

namespace In_Memory_Database_Testing
{
    public class SavingAndLoadingDatabaseTest
    {
        ITestOutputHelper testOutputHelper;

        public SavingAndLoadingDatabaseTest(ITestOutputHelper outputHelper)
        {
            testOutputHelper = outputHelper;
        }

        [Fact]
        public async void SavingAndLoading_ExpectSameInfosBeforeChange()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable("AgeTable", new List<string> { "Age" }, new List<Type> { typeof(int) });

            await db["AgeTable"].AddRow(new DataRow { 1 });
            await db["AgeTable"].AddRow(new DataRow { 2 });

            db.Begin();
            await db["AgeTable"].AddRow(new DataRow { 3 });
            await db["AgeTable"].AddRow(new DataRow { 4 });
            db.Abort();

            await db["AgeTable"].CreateIndex("Age");

            var indexedSearchResults = await db["AgeTable"]
                .Search(new SearchConditions("Age", ">=", 0));

            //Act
            db.SetDiskLocation("backup");
            db.SaveToDisk();

            var newDb = new Database(new SearchManager(), new DiskManager());
            newDb.SetDiskLocation("backup");
            newDb.LoadFromDisk();
            var indexedSearchResults2 = await newDb["AgeTable"]
                .Search(new SearchConditions("Age", ">=", 0));

            //Assert
            Assert.True(File.Exists("backup/AgeTable.json"));
            Assert.Single(newDb["AgeTable"].IndexTables);
            Assert.Equal(2, newDb["AgeTable"].Rows.Count());

            //This checks that indextable and the search manager are working properly
            Assert.Equal(2, indexedSearchResults.Count);
            Assert.Equal(2, indexedSearchResults2.Count);
        }
    }
}
