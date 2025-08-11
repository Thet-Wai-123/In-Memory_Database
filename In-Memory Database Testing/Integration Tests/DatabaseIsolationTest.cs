using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using In_Memory_Database.Classes.Data;
using In_Memory_Database.Classes.Dependencies.Managers;

namespace In_Memory_Database_Testing.Integration_Tests
{
    public class DatabaseIsolationTest
    {
        [Fact]
        public async Task ReadOnlyCommittedInConcurrentTransactions_ExpectIsolation()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable("Student", new List<string> { "Id" }, new List<Type> { typeof(int) });

            var t1Started = new TaskCompletionSource<bool>();
            var t1Commited = new TaskCompletionSource<bool>();
            var t2ReadHeight = new TaskCompletionSource<bool>();
            var heightInTransaction1 = -1;
            var heightInTransaction2 = -1;
            var heightInTransaction2AfterCommit = -1;

            //Act
            var transaction1 = Task.Run(async () =>
            {
                db.Begin();
                await db["Student"].AddRow(new DataRow { 1 });
                heightInTransaction1 = db["Student"].Height;
                t1Started.SetResult(true);
                await t2ReadHeight.Task;
                db.Commit();
                t1Commited.SetResult(true);
            });

            var transaction2 = Task.Run(async () =>
            {
                await t1Started.Task;
                db.Begin();
                heightInTransaction2 = db["Student"].Height;
                t2ReadHeight.SetResult(true);

                await t1Commited.Task;
                heightInTransaction2AfterCommit = db["Student"].Height;
            });

            await Task.WhenAll(transaction1, transaction2);

            //Assert
            Assert.Equal(1, heightInTransaction1);
            Assert.Equal(0, heightInTransaction2);
            Assert.Equal(1, heightInTransaction2AfterCommit);
        }

        [Fact(Timeout = 5000)]
        public async Task WriteNeverBlocksRead_ExpectForReadToGoThroughWhileWriting()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable("Student", new List<string> { "Id" }, new List<Type> { typeof(int) });
            var t1Started = new TaskCompletionSource<bool>();

            //Act
            var transaction1 = Task.Run(async () =>
            {
                db.Begin();
                await db["Student"].AddRow(new DataRow { 1 });
                t1Started.SetResult(true);
                //Don't commit, so it'll hold the row exclusive lock
            });

            await t1Started.Task;
            await db["Student"].Search(new SearchConditions("Id", "==", 1));

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
            await db["Student"].AddRow(new DataRow { 1, "John0" });

            var t1Started = new TaskCompletionSource<bool>();
            var t2Started = new TaskCompletionSource<bool>();

            //Act
            var transaction1 = Task.Run(async () =>
            {
                db.Begin();
                await db["Student"].UpdateRow(new SearchConditions("Id", "==", 1), "Name", "John1");
                t1Started.SetResult(true);
                await Task.Delay(1000);
                db.Commit();
                orderOfCompletion.Add(1);
            });

            var transaction2 = Task.Run(async () =>
            {
                await t1Started.Task;
                db.Begin();
                await db["Student"].UpdateRow(new SearchConditions("Id", "==", 1), "Name", "John2");
                db.Commit();
                orderOfCompletion.Add(2);
            });

            await Task.WhenAll(transaction1, transaction2);

            //Assert
            Assert.Equal([1, 2], orderOfCompletion);
            var result = await db["Student"].Search(new SearchConditions("Id", "==", 1));
            Assert.Single(result);
            Assert.Equal([1, "John2"], result[0]);
        }

        [Fact]
        public async Task ExclusiveTableLockConflictsWithOtherLocks_ExpectCorrectOrder()
        {
            //Arrange
            var db = new Database(new SearchManager(), new DiskManager());
            db.CreateTable("Student", new List<string> { "Id" }, new List<Type> { typeof(int) });

            //Helpers to help with test
            var orderOfCompletion = new List<string>();
            var readTransactionInProgress = new TaskCompletionSource<bool>();

            //Act
            var rowExclusive = Task.Run(async () =>
            {
                db.Begin();
                await db["Student"].AddRow([1]);
                readTransactionInProgress.SetResult(true);
                await Task.Delay(1000);
                db.Commit();
                orderOfCompletion.Add("Add Row");
            });

            var accessExclusive = Task.Run(async () =>
            {
                await readTransactionInProgress.Task;
                await db["Student"].AddColumn("Name", typeof(String));
                orderOfCompletion.Add("Add Column");
            });

            await Task.WhenAll(rowExclusive, accessExclusive);

            //Assert
            Assert.Equal("Add Column", orderOfCompletion[1]);
        }
    }
}
