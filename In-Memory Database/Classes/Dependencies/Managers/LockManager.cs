using DotNext.Threading;
using In_Memory_Database.Classes.Data;
using In_Memory_Database.Helpers;
using System.Collections.Concurrent;

namespace In_Memory_Database.Classes.Dependencies.Managers
{
    //This is more for shared locks which will be shared and be called at the end of commit or abort.
    internal static class LockManager
    {
        //Each datatable should have one lock
        static ConcurrentDictionary<IDataTable, AsyncReaderWriterLock> rwlocks = new();

        //Xid => lock it holds (This makes sure that each context will only call to lock on rwlocks[dt] once, meaning no recursive for any single context)
        static ConcurrentDictionary<
            long,
            ConcurrentDictionary<IDataTable, LockType>
        > xidToHeldLocks = new();

        static BidirectionalConcurrentMap rowsToTransactionsMap = new();

        internal enum LockType
        {
            AccessShareLock, //Read
            RowExclusiveLock, //Write Rows
            AccessExclusiveLock //Change Structure
        }

        internal static async Task GetLock(
            LockType lockType,
            IDataTable dt,
            long xid,
            DataRow? dataRow = null
        )
        {
            rwlocks.TryAdd(dt, new AsyncReaderWriterLock());

            var heldLocks = xidToHeldLocks.GetOrAdd(
                xid,
                _ => new ConcurrentDictionary<IDataTable, LockType>()
            );

            if (lockType == LockType.AccessExclusiveLock)
            {
                if (heldLocks.TryGetValue(dt, out var existingLock))
                {
                    //Already has a lock, check if it's lower than AccessExclusive
                    if (existingLock != LockType.AccessExclusiveLock)
                    {
                        //Upgrade lock and update mapping
                        await rwlocks[dt].UpgradeToWriteLockAsync();
                        heldLocks[dt] = LockType.AccessExclusiveLock;
                    }
                }
                else
                {
                    await rwlocks[dt].EnterWriteLockAsync();
                    heldLocks.TryAdd(dt, LockType.AccessExclusiveLock);
                }
            }
            else if (lockType == LockType.RowExclusiveLock)
            {
                if (dataRow == null)
                    throw new ArgumentNullException("Target row to lock is missing");

                //You already own a lock to this table, allowed to pass
                if (heldLocks.ContainsKey(dt))
                    return;

                //Another transaction has the lock on this row
                while (rowsToTransactionsMap.ContainsRowId(dataRow.Tid))
                {
                    await Task.Delay(10);
                }
                await rwlocks[dt].EnterReadLockAsync();
                rowsToTransactionsMap.Add(dataRow.Tid, xid);
                heldLocks.TryAdd(dt, LockType.RowExclusiveLock);
            }
            else if (lockType == LockType.AccessShareLock)
            {
                //You already own a lock to this table, allowed to pass
                if (heldLocks.ContainsKey(dt))
                {
                    return;
                }

                //Simulate how it instantaly release the AccessShareLock as soon as it is done reading. This checks that table is not locked by exclusive lock
                await rwlocks[dt].EnterReadLockAsync();
                rwlocks[dt].Release();
            }
        }

        internal static void Release(long xid)
        {
            //Remove the mapping
            if (xidToHeldLocks.Remove(xid, out var heldLocks))
            {
                foreach (var affectedDt in heldLocks.Keys)
                {
                    if (rwlocks.TryGetValue(affectedDt, out var rwlock))
                    {
                        rwlock.Release();
                    }
                    else
                    {
                        //There must be a lock in rwlocks because the outerloop is getting all affected datatables, meaning this has the lock otherwise a resource will be stuck.
                        throw new Exception("A lock is not being properly released");
                    }
                }
            }

            //Free the tracked row if it is applicable
            rowsToTransactionsMap.TryRemoveByXid(xid);
        }
    }
}
