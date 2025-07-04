using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using In_Memory_Database.Classes.Dependencies.Managers;
using Xunit.Abstractions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace In_Memory_Database_Testing.Unit_Tests
{
    public class TransactionManagerTest
    {
        //For debugging, delete later
        private readonly ITestOutputHelper _testOutputHelper;

        public TransactionManagerTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        //end of debugging

        [Fact]
        public async Task UniqueTransactionIdAssignedConcurrently_ExpectsToPersistForDifferentThreads()
        {
            var task1 = Task.Run(async () =>
            {
                var tm = new TransactionManager();
                tm.Begin();
                var curTransaction1 = TransactionManager.GetCurrentTransaction();
                _testOutputHelper.WriteLine($"Task 1 Transaction Xmin: {curTransaction1.Xmin}");

                //works with async and waiting 1 sec for task 2 to run.
                var curTransaction2 = TransactionManager.GetCurrentTransaction();
                await Task.Delay(1000);
                _testOutputHelper.WriteLine(
                    $"Task 1 Transaction Xmin after waiting: {curTransaction2.Xmin}"
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
