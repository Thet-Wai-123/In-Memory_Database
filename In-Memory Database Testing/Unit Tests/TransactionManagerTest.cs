using In_Memory_Database.Classes.Dependencies.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
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

        [Fact]
        public void SameThreadReusedForAnotherTransaction_ExpectTheExecutionContextToWorkProperly()
        {
            //Arrange
            TransactionManager.Begin();
            var originalTransaction = TransactionManager.GetCurrentTransaction();

            //Act
            var beginException = Record.Exception(() => TransactionManager.Begin());
            TransactionManager.Commit();
            var secondBeginException = Record.Exception(() => TransactionManager.Begin());

            //Assert
            Assert.Equal("Already ongoing transaction", beginException!.Message);
            Assert.Null(secondBeginException);
        }
    }
}
