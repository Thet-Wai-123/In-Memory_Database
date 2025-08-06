using System.Collections.Concurrent;

namespace In_Memory_Database.Helpers
{
    public class BidirectionalConcurrentMap
    {
        private readonly ConcurrentDictionary<Guid, long> _rowToXid = new();
        private readonly ConcurrentDictionary<long, Guid> _xidToRow = new();

        public void Add(Guid rowId, long xid)
        {
            _rowToXid[rowId] = xid;
            _xidToRow[xid] = rowId;
        }

        public bool TryRemoveByXid(long xid)
        {
            if (_xidToRow.TryRemove(xid, out var rowId))
            {
                _rowToXid.TryRemove(rowId, out _);
                return true;
            }
            return false;
        }

        public bool ContainsRowId(Guid tid)
        {
            return _rowToXid.ContainsKey(tid);
        }
    }
}
