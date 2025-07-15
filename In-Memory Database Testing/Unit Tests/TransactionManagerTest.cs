using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using In_Memory_Database.Classes.Dependencies.Managers;
using Xunit.Abstractions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace In_Memory_Database_Testing
{
    public class TransactionManagerTest
    {
        //For printing out messages
        private readonly ITestOutputHelper _testOutputHelper;

        public TransactionManagerTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task UniqueTransactionInDifferentTasks_ExpectsToBeSameTransaction()
        {
            var task1 = Task.Run(async () =>
            {
                TransactionManager.Begin();
                var curTransaction1 = TransactionManager.GetCurrentTransaction();
                _testOutputHelper.WriteLine($"Task 1 Transaction id: {curTransaction1.xid}");

                //waiting 1 sec for task 2 to run.
                var curTransaction2 = TransactionManager.GetCurrentTransaction();
                await Task.Delay(1000);
                _testOutputHelper.WriteLine(
                    $"Task 1 Transaction id after task 2: {curTransaction2.xid}"
                );
                Assert.Equal(curTransaction1, curTransaction2);
            });
            var task2 = Task.Run(async () =>
            {
                //Delay of 0.5 to make sure task 2 is ran a little bit after.
                await Task.Delay(500);
                TransactionManager.Begin();
                var curTransaction = TransactionManager.GetCurrentTransaction();
                _testOutputHelper.WriteLine($"Task 2 Transaction id: {curTransaction.xid}");
            });

            await Task.WhenAll(task1, task2);
        }

        //By default, the asynclocal shallow copies to the new nested async task. We can use that to prevent nested transactions.
        //This test also makes sure that if a transaction is handled again using the same thread, the old transaction MUST commit in order to work properly.
        [Fact]
        public void SameThreadReusedForAnotherTransaction_ExpectTheExecutionContextToWorkProperly()
        {
            var firstException = "";
            long secondTransactionId = 0;

            var thread1 = new Thread(() =>
            {
                TransactionManager.Begin();
                var curTransaction1 = TransactionManager.GetCurrentTransaction();

                var thread2 = new Thread(() =>
                {
                    try
                    {
                        TransactionManager.Begin();
                    }
                    catch (Exception ex)
                    {
                        firstException = ex.Message;
                    }
                });
                thread2.Start();
                thread2.Join();

                //commits our original transaction
                TransactionManager.Commit();

                //Now should be able to reuse the same thread for another transaction
                var thread3 = new Thread(() =>
                {
                    TransactionManager.Begin();
                    var curTransaction = TransactionManager.GetCurrentTransaction();
                    secondTransactionId = curTransaction.xid;
                });
                thread3.Start();
                thread3.Join();
            });

            thread1.Start();
            thread1.Join();

            Assert.Equal("Already ongoing transaction", firstException);
            Assert.Equal(2, secondTransactionId);
        }
    }
}
