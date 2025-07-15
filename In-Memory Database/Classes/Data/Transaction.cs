using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In_Memory_Database.Classes.Data
{
    public class Transaction
    {
        public long xid { get; set; }
        public bool IsCommitted { get; set; }

        public Transaction(long xid)
        {
            this.xid = xid;
            IsCommitted = false;
        }
    }
}
