using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace In_Memory_Database.Classes.Dependencies.Managers
{
    //This is more for shared locks which will be shared and be called at the end of commit or abort.
    internal static class LockManager
    {
        //xid, lock object
        private static ConcurrentDictionary<long, object> _lockDict;

        internal static void Release(long xid)
        {
            //Monitor.Exit(_lockDict[xid]);
        }
    }
}
