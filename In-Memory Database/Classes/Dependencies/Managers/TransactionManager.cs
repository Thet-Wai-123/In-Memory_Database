using In_Memory_Database.Classes.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace In_Memory_Database.Classes.Dependencies.Managers
{
    public class TransactionManager
    {
        private static long _nextId = 1;
        private static readonly object _nextTransactionIdLock = new();
        private static readonly AsyncLocal<Transaction?> _currentTransaction = new();
        private static readonly ConcurrentDictionary<long, Transaction> _allTransactionsStatus =
            new();

        internal static void Begin()
        {
            if (_currentTransaction.Value != null)
            {
                throw new InvalidOperationException("Already ongoing transaction");
            }
            Transaction newTransaction;
            lock (_nextTransactionIdLock)
            {
                newTransaction = new Transaction(_nextId);
                _currentTransaction.Value = newTransaction;
                _nextId++;
            }
            _allTransactionsStatus.TryAdd(newTransaction.xid, newTransaction);
        }

        internal static long PeekTopId()
        {
            return _nextId;
        }

        internal static void Commit()
        {
            if (_currentTransaction.Value == null)
            {
                throw new InvalidOperationException("No active transaction");
            }

            var curTransaction = _currentTransaction.Value;
            curTransaction.IsCommitted = true;
            _currentTransaction.Value = null;

            LockManager.Release(curTransaction.xid);
        }

        internal static void RollBack()
        {
            if (_currentTransaction.Value == null)
            {
                throw new InvalidOperationException("No active transaction");
            }
            var curTransaction = _currentTransaction.Value;
            _allTransactionsStatus.TryRemove(curTransaction.xid, out _);
            _currentTransaction.Value = null;

            LockManager.Release(curTransaction.xid);
        }

        //it'll get called from different isolated scopes, and the transaction it'll get will be different.
        internal static Transaction? GetCurrentTransaction()
        {
            return _currentTransaction.Value;
        }

        internal static bool checkTransactionStatus(long xid)
        {
            //Special id resetted, in a way this is like freezing in psql to reset and wrap if necessary.
            if (xid == 0)
            {
                return true;
            }

            if (_allTransactionsStatus.ContainsKey(xid))
            {
                return _allTransactionsStatus[xid].IsCommitted;
            }
            else
                return false;
        }
    }
}
