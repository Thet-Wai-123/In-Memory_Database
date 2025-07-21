using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In_Memory_Database_Testing.Integration_Tests
{
    public class DatabaseAtomicityTest
    {
        [Fact]
        public async Task ReadOnlyCommittedInConcurrentTransactions_ExpectIsolation()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable("Student", new List<string> { "Id" }, new List<Type> { typeof(int) });

            var commited = false;
            var heightInTransaction1 = -1;
            var heightInTransaction2 = -1;
            var heightInTransaction2AfterCommit = -1;

            //Act
            var transaction1 = Task.Run(async () =>
            {
                db.Begin();
                db["Student"].AddRow(new DataRow { 1 });
                heightInTransaction1 = db["Student"].Height;
                await Task.Delay(1000);
                db.Commit();
                commited = true;
            });

            //Small delay to make t2 start shortly after t1
            await Task.Delay(10);

            var transaction2 = Task.Run(async () =>
            {
                db.Begin();
                heightInTransaction2 = db["Student"].Height;

                while (!commited)
                {
                    await Task.Delay(10);
                }
                heightInTransaction2AfterCommit = db["Student"].Height;
            });

            await Task.WhenAll(transaction1, transaction2);

            //Assert
            Assert.Equal(1, heightInTransaction1);
            Assert.Equal(0, heightInTransaction2);
            Assert.Equal(1, heightInTransaction2AfterCommit);
        }

        [Fact]
        public async Task AbortingTransactionRevertsAllChanges_ExpectsOriginalData()
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

        [Fact(Timeout = 5000)]
        public async Task WriteNeverBlocksRead_ExpectForReadToGoThroughWhileWriting()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable("Student", new List<string> { "Id" }, new List<Type> { typeof(int) });

            //Act
            var transaction1 = Task.Run(async () =>
            {
                db.Begin();
                db["Student"].AddRow(new DataRow { 1 });
                //Don't commit, so it'll hold the row exclusive lock
            });

            //Small delay to make t2 start shortly after t1
            await Task.Delay(10);
            db["Student"].Search(new SearchConditions("Id", "==", 1));

            //Assert
            Assert.True(true, "Finished the task in time meaning not blocked");
        }

        [Fact]
        public async Task WritingToSameRow_ExpectsConflictInLocksAndWait()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable(
                "Student",
                new List<string> { "Id", "Name" },
                new List<Type> { typeof(int), typeof(String) }
            );
            var orderOfCompletion = new List<int>();
            db["Student"].AddRow(new DataRow { 1, "John0" });

            //Act
            var transaction1 = Task.Run(async () =>
            {
                db.Begin();
                db["Student"].UpdateRow(new SearchConditions("Id", "==", 1), "Name", "John1");
                await Task.Delay(1000);
                db.Commit();
                orderOfCompletion.Add(1);
            });

            //Small delay to make t2 start shortly after t1
            await Task.Delay(10);

            var transaction2 = Task.Run(async () =>
            {
                db.Begin();
                db["Student"].UpdateRow(new SearchConditions("Id", "==", 1), "Name", "John2");
                db.Commit();
                orderOfCompletion.Add(2);
            });

            await Task.WhenAll(transaction1, transaction2);

            //Assert
            Assert.Equal([1, 2], orderOfCompletion);
            var result = db["Student"].Search(new SearchConditions("Id", "==", 1));
            Assert.Equal([1, "John2"], result[0]);
        }

        [Fact]
        public async Task ChangingTableStructureLocksTheTable_ExpectAllOperationsToBeLocked()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable("Student", new List<string> { "Id" }, new List<Type> { typeof(int) });
            var orderOfCompletion = new List<int>();

            //Act
            //Below, t1 is delayed by 1 sec, but t2 and t3 waits. t2 and t3 should not block each other, so it doesn't matter what is faster. Only the first to complete matters.
            var transaction1 = Task.Run(async () =>
            {
                db.Begin();
                db["Student"].AddColumn("Name", typeof(String));
                await Task.Delay(1000);
                db.Commit();
                orderOfCompletion.Add(1);
            });

            //Small delay to make t2 and t3 start shortly after t1
            await Task.Delay(10);

            var transaction2 = Task.Run(async () =>
            {
                db.Begin();
                db["Student"].Search(new SearchConditions("Id", "==", 1));
                orderOfCompletion.Add(2);
            });

            var transaction3 = Task.Run(async () =>
            {
                db.Begin();
                db["Student"].AddRow(new DataRow { 1, "John" });
                orderOfCompletion.Add(3);
            });

            await Task.WhenAll(transaction1, transaction2, transaction3);

            //Assert
            Assert.Equal(1, orderOfCompletion[0]);
        }
    }
}
