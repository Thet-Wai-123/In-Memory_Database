using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In_Memory_Database.Classes.Data
{
    internal class DataRowVersion
    {
        public DataRowVersion(long xmin, long xmax)
        {
            Xmin = xmin;
            Xmax = xmax;
        }

        public long Xmin
        {
            get;
        }
        public long Xmax
        {
            get; set;
        }
    }
}
