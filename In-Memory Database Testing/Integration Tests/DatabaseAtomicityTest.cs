using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace In_Memory_Database_Testing.Integration_Tests
{
    public class DatabaseAtomicityTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DatabaseAtomicityTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task AbortingRowChangesRevertsWholeTransaction_ExpectsOriginalRows()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable("Student", new List<string> { "Id" }, new List<Type> { typeof(int) });

            //Act
            db.Begin();
            await db["Student"].AddRow(new DataRow { 1 });
            var heightBeforeAbort = db["Student"].Height;
            db.Abort();

            //Assert
            Assert.Equal(1, heightBeforeAbort);
            Assert.Equal(0, db["Student"].Height);
        }

        [Fact]
        public async Task BankExampleAtomicity_ExpectsAtomicityAcrossDifferentTables()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable(
                "Bank",
                new List<string> { "Owner", "Amount" },
                new List<Type> { typeof(int), typeof(int) }
            );
            db.CreateTable(
                "Bank2",
                new List<string> { "Owner", "Amount" },
                new List<Type> { typeof(int), typeof(int) }
            );

            //Starting Amount
            await db["Bank"].AddRow(new DataRow { 1, 1000 });
            await db["Bank2"].AddRow(new DataRow { 1, 0 });

            //Act
            try
            {
                db.Begin();

                //Withdraw 500
                var curAmountInBank1 = (
                    await db["Bank"].Search(new SearchConditions("Owner", "==", 1))
                )[0][1];
                await db["Bank"]
                    .UpdateRow(
                        new SearchConditions("Owner", "==", 1),
                        "Amount",
                        curAmountInBank1 - 500
                    );

                //Deposit 500
                var curAmountInBank2 = (
                    await db["Bank2"].Search(new SearchConditions("Owner", "==", 1))
                )[0][1];
                await db["Bank2"]
                    .UpdateRow(
                        new SearchConditions("Owner", "==", 1),
                        "Amount",
                        curAmountInBank2 + 500
                    );

                throw new Exception("Crashed");

                db.Commit();
            }
            catch
            {
                db.Abort();
            }

            //Assert
            var finalBank = (await db["Bank"].Search(new SearchConditions("Owner", "==", 1)))[0];
            var finalBank2 = (await db["Bank2"].Search(new SearchConditions("Owner", "==", 1)))[0];

            Assert.Equal(1000, finalBank[1]);
            Assert.Equal(0, finalBank2[1]);
        }
    }
}
