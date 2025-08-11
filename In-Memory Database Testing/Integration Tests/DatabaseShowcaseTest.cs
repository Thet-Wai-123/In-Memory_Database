using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In_Memory_Database_Testing.Integration_Tests
{
    public class DatabaseShowcaseTest
    {
        [Fact]
        public async void DatabaseShowcase()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable(
                "Student",
                new List<string> { "Name", "Age", "Score" },
                new List<Type> { typeof(string), typeof(int), typeof(int) }
            );

            //Act
            await db["Student"].AddRow(new DataRow { "A", 18, 100 });
            await db["Student"].AddRow(new DataRow { "B", 18, 90 });
            await db["Student"].AddRow(new DataRow { "C", 18, 80 });
            await db["Student"].AddRow(new DataRow { "D", 18, 70 });

            //Create index on score
            await db["Student"].CreateIndex("Score");
            var indexes = db["Student"].IndexTables;

            //removing all scores less than 90
            await db["Student"].RemoveRow(new SearchConditions("Score", "<=", 90));

            //Assert
            Assert.Single(indexes);
            Assert.Equal(1, db["Student"].Height);
        }
    }
}
