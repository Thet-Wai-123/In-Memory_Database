using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using In_Memory_Database;
using In_Memory_Database.Classes;
using Microsoft.Extensions.DependencyInjection;

namespace In_Memory_Database_Testing
{
    public class DatabaseTest
    {
        [Fact]
        public void SavingAndLoading_ExpectSameInfosBeforeChange()
        {
            //Arrange
            var db = new Database(new SearchManager(), new FileManager());
            db.CreateTable("AgeTable", new List<string> { "Age" }, new List<Type> { typeof(int) });
            db.AddRow("AgeTable", [20]);
            db.AddRow("AgeTable", [21]);
            db.AddRow("AgeTable", [22]);
            db.AddRow("AgeTable", [23]);

            //Act
            db.SaveToDisk();

            db.ClearDb();
            db.LoadFromDisk();

            //Assert
            Assert.True(File.Exists(db.storageLocation + "/AgeTable.json"));
            Assert.Equal(4, db.GetTable("AgeTable").Rows.Count());
        }
    }
}
