using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using In_Memory_Database.Classes.Data;

namespace In_Memory_Database.Classes.Dependencies.Managers
{
    public class TransactionManager
    {
        private static long _nextId = 1;
        private static readonly object _nextIdLock = new();
        private static AsyncLocal<Transaction> _currentTransaction = new();
        private static ConcurrentDictionary<long, Transaction> _allTransactionsStatus = new();

        public Transaction Begin()
        {
            if (_currentTransaction.Value != null)
            {
                throw new InvalidOperationException("Already has an ongoing transaction");
            }
            Transaction newTransaction;
            lock (_nextIdLock)
            {
                newTransaction = new Transaction(_nextId);
                _currentTransaction.Value = newTransaction;
                _nextId++;
            }
            _allTransactionsStatus.TryAdd(newTransaction.Xmin, newTransaction);
            return newTransaction;
        }

        public void Commit()
        {
            if (_currentTransaction.Value != null)
            {
                throw new InvalidOperationException("No active transaction");
            }

            var curTransaction = _currentTransaction.Value;
            curTransaction.IsCommitted = true;
            _currentTransaction.Value = null;
        }

        //For now, when rolling back, we'll just remove it permantely for now, which helps save a lot of space, and easier
        public void RollBack()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction");
            }
            var curTransaction = _currentTransaction.Value;
            _allTransactionsStatus.TryRemove(curTransaction.Xmin, out _);
            _currentTransaction.Value = null;
        }

        //This is static because it'll get called from different isolated scopes, and the transaction it'll get will be different.
        internal static Transaction GetCurrentTransaction()
        {
            return _currentTransaction.Value;
        }
    }
}
