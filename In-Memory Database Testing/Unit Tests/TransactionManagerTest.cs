using In_Memory_Database.Classes.Dependencies.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace In_Memory_Database_Testing.Unit_Tests
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
        public async Task UniqueTransactionInOriginalAndNewTask_ExpectsToBeSameTransaction()
        {
            //Arrange
            //Happening in original thread
            var tm = new TransactionManager();
            tm.Begin();
            var curTransactionStart = TransactionManager.GetCurrentTransaction();
            _testOutputHelper.WriteLine(
                $"Task 1 Transaction Xmin at start: {curTransactionStart.Xmin}"
            );

            //There is another concurrent session going on in another thread
            await Task.Run(() =>
            {
                var tm2 = new TransactionManager();
                tm2.Begin();
                var curTransaction2 = TransactionManager.GetCurrentTransaction();
                _testOutputHelper.WriteLine(
                    $"Task 2 Transaction Xmin at start: {curTransaction2.Xmin}"
                );
            });

            //Act
            var curTransactionEnd = TransactionManager.GetCurrentTransaction();
            _testOutputHelper.WriteLine(
                $"Task 1 Transaction Xmin at end: {curTransactionEnd.Xmin}"
            );

            //Assert
            Assert.Equal(curTransactionStart, curTransactionEnd);
        }

        [Fact]
        public async Task UniqueTransactionInDifferentTasks_ExpectsToBeSameTransaction()
        {
            var task1 = Task.Run(async () =>
            {
                var tm = new TransactionManager();
                tm.Begin();
                var curTransaction1 = TransactionManager.GetCurrentTransaction();
                _testOutputHelper.WriteLine($"Task 1 Transaction Xmin: {curTransaction1.Xmin}");

                //waiting 1 sec for task 2 to run.
                var curTransaction2 = TransactionManager.GetCurrentTransaction();
                await Task.Delay(1000);
                _testOutputHelper.WriteLine(
                    $"Task 1 Transaction Xmin at end: {curTransaction2.Xmin}"
                );
                Assert.Equal(curTransaction1, curTransaction2);
            });
            var task2 = Task.Run(async () =>
            {
                //Delay of 0.5 to make sure task 2 is ran a little bit after.
                await Task.Delay(500);
                var tm = new TransactionManager();
                tm.Begin();
                var curTransaction = TransactionManager.GetCurrentTransaction();
                _testOutputHelper.WriteLine($"Task 2 Transaction Xmin: {curTransaction.Xmin}");
            });

            await Task.WhenAll(task1, task2);
        }
    }
}
