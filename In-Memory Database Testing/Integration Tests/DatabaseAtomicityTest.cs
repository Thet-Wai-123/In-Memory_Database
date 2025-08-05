using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In_Memory_Database_Testing.Integration_Tests
{
    public class DatabaseAtomicityTest
    {
        [Fact]
        public async Task AbortingRowChangesRevertsWholeTransaction_ExpectsOriginalRows()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable("Student", new List<string> { "Id" }, new List<Type> { typeof(int) });

            //Act
            db.Begin();
            db["Student"].AddRow(new DataRow { 1 });
            var heightBeforeAbort = db["Student"].Height;
            db.Abort();

            //Assert
            Assert.Equal(1, heightBeforeAbort);
            Assert.Equal(0, db["Student"].Height);
        }
    }
}
