using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;

namespace In_Memory_Database_Testing
{
    public class SavingAndLoadingTest
    {
        [Fact]
        public void SavingAndLoading_ExpectSameInfosBeforeChange()
        {
            //Arrange
            var db = new Database(new SearchManager(), new FileManager());
            db.CreateTable("AgeTable", new List<string> { "Age" }, new List<Type> { typeof(int) });
            db["AgeTable"].AddRow(new DataRow { 1 });
            db["AgeTable"].AddRow(new DataRow { 2 });
            db["AgeTable"].AddRow(new DataRow { 3 });
            db["AgeTable"].AddRow(new DataRow { 4 });

            //Act
            db.SaveToDisk();

            db.ClearDb();
            db.LoadFromDisk();

            //Assert
            Assert.True(File.Exists(db.storageLocation + "/AgeTable.json"));
            Assert.Equal(4, db["AgeTable"].Rows.Count());
        }
    }
}
