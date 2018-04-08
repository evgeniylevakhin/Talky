using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class GenerateError
    {
        public static void CheckAndGenerate(string msg)
        {


            if (msg.Contains("!error_exception"))
            {
                throw new System.Exception("Unhandled exception");
            }
            else if (msg.Contains("!error_overflow"))
            {
                CauseOverflow();
            }
            else if (msg.Contains("!error_oom"))
            {
                CauseOutOfMemory();
            }
        }

        public static void CauseOverflow()
        {
            CauseOverflow();
        }

        public static void CauseOutOfMemory()
        {
            var sb = new StringBuilder(15, 15);
            sb.Append("Substring #1 ");
            sb.Insert(0, "Substring #2 ", 1);
        }
    }
}
