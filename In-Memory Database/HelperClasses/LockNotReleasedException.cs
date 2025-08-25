using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In_Memory_Database.HelperClasses
{
    public class LockNotReleasedException :Exception
    {
        public LockNotReleasedException(string message)
            : base(message) { }
    }
}
