using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In_Memory_Database.Classes.Data
{
    public class Transaction
    {
        public long Xmin { get; set; }
        public long? Xmax { get; set; }
        public bool IsCommitted { get; set; }

        public Transaction(long tid)
        {
            Xmin = tid;
            Xmax = null;
            IsCommitted = false;
        }
    }
}
