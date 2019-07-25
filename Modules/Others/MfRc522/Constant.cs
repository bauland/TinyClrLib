using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Bauland.Others
{
    namespace Constants.MfRc522
    {
        public enum Register
        {
            // Command and status
            Command = 0x01 << 1,
            ComIrq = 0x04 << 1,
            DivIrq = 0x05 << 1,
            Error = 0x06 << 1,
            FifoData = 0x09 << 1,
            FifoLevel = 0x0A << 1,
            Control = 0x0C << 1,
            BitFraming = 0x0D << 1,
            Coll = 0x0E << 1,

            Mode = 0x11 << 1,
            TxMode = 0x12 << 1,
            RxMode = 0x13 << 1,
            TxControl = 0x14 << 1,
            TxASK = 0x15 << 1,
            Version = 0x37 << 1,

            CrcResultHigh = 0x21 << 1,
            CrcResultLow = 0x22 << 1,
            ModeWith = 0x24 << 1,
            TMode = 0x2A << 1,
            TPrescaler = 0x2B << 1,
            TReloadH = 0x2C << 1,
            TReloadL = 0x2D << 1,
        }

        public enum StatusCode
        {
            Ok,
            Collision,
            Error,
            Timeout,
            NoRoom
        }

        public enum PiccCommand
        {
            ReqA = 0x26,
            HaltA = 0x50,
            SelCl1 = 0x93,
            SelCl2 = 0x95,
            SelCl3 = 0x97,
        }

        public enum PcdCommand
        {
            Transceive = 0x0c,
            Idle = 0x00,
            CalculateCrc = 0x03,
        }

        public enum UidType
        {
            T4 = 4,
            T7 = 7,
            T10 = 10
        }

        public enum PiccType
        {
            Unknown,
            Mifare1K,
            MifareUltralight
        }

        public class Uid
        {
            public UidType UidType { get; set; }
            public byte[] UidBytes { get; set; }
            public byte Sak { get; set; }

            public PiccType GetPiccType()
            {
                var sak = Sak & 0x7f;
                switch (sak)
                {
                    case 0x08:
                        return PiccType.Mifare1K;
                    case 0x00:
                        return PiccType.MifareUltralight;
                    default:
                        return PiccType.Unknown;
                }
            }
        }
    }
}
