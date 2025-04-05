using System.Reflection;
using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using Newtonsoft.Json;

namespace In_Memory_Database_Testing
{
    public class SavingAndLoadingDatabaseTest
    {
        [Fact]
        public void SavingAndLoading_ExpectSameInfosBeforeChange()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable("AgeTable", new List<string> { "Age" }, new List<Type> { typeof(int) });
            db["AgeTable"].AddRow(new DataRow { 1 });
            db["AgeTable"].AddRow(new DataRow { 2 });
            db["AgeTable"].AddRow(new DataRow { 3 });
            db["AgeTable"].AddRow(new DataRow { 4 });

            //Act
            db.SetDiskLocation("backup");
            db.SaveToDisk();

            var newDb = new Database(new SearchManager(), new DiskManager());
            newDb.SetDiskLocation("backup");
            newDb.LoadFromDisk();
            var newDbSearch = newDb["AgeTable"].Search(new SearchConditions("Age", "==", 1));

            //Assert
            Assert.True(File.Exists("backup/AgeTable.json"));
            Assert.Equal(4, newDb["AgeTable"].Rows.Count());
            Assert.NotNull(newDbSearch);
        }
    }
}
