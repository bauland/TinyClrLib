using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Bauland.Others
{
    namespace Constants.MfRc522
    {
        public static class Register
        {
            // Command and status
            public static byte Command = 0x01 << 1;
            public static byte TxASK = 0x15 << 1;
            public static byte Mode = 0x11 << 1;
        }
    }
}
