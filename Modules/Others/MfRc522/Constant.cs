// ReSharper disable UnusedMember.Global
#pragma warning disable 1591

namespace Bauland.Others
{
    namespace Constants.MfRc522
    {
        /// <summary>
        /// Address of register
        /// </summary>
        public enum Register
        {
            // Command and status
            Command = 0x01 << 1,
            ComIrq = 0x04 << 1,
            DivIrq = 0x05 << 1,
            Error = 0x06 << 1,
            Status1 = 0x07 << 1,
            Status2 = 0x08 << 1,
            FifoData = 0x09 << 1,
            FifoLevel = 0x0A << 1,
            Control = 0x0C << 1,
            BitFraming = 0x0D << 1,
            Coll = 0x0E << 1,

            Mode = 0x11 << 1,
            TxMode = 0x12 << 1,
            RxMode = 0x13 << 1,
            TxControl = 0x14 << 1,
            TxAsk = 0x15 << 1,
            Version = 0x37 << 1,

            CrcResultHigh = 0x21 << 1,
            CrcResultLow = 0x22 << 1,
            ModeWith = 0x24 << 1,
            TimerMode = 0x2A << 1,
            TimerPrescaler = 0x2B << 1,
            TimerReloadHigh = 0x2C << 1,
            TimerReloadLow = 0x2D << 1,
        }

        /// <summary>
        /// Return code of some functions
        /// </summary>
        public enum StatusCode
        {
            Ok,
            Collision,
            Error,
            Timeout,
            NoRoom,
            CrcError
        }

        /// <summary>
        /// Command to send to picc (card)
        /// </summary>
        public enum PiccCommand
        {
            ReqA = 0x26,
            MifareRead = 0x30,
            HaltA = 0x50,
            AuthenticateKeyA = 0x60,
            AuthenticateKeyB = 0x61,
            SelCl1 = 0x93,
            SelCl2 = 0x95,
            SelCl3 = 0x97,
        }

        /// <summary>
        /// Command of reader
        /// </summary>
        public enum PcdCommand
        {
            Idle = 0x00,
            CalculateCrc = 0x03,
            Transceive = 0x0c,
            MfAuthenticate = 0xe,
        }

        /// <summary>
        /// Length of uid
        /// </summary>
        public enum UidType
        {
            T4 = 4,
            T7 = 7,
            T10 = 10
        }

        /// <summary>
        /// Type of card
        /// </summary>
        public enum PiccType
        {
            Unknown,
            Mifare1K,
            MifareUltralight
        }

        /// <summary>
        /// 
        /// </summary>
        public class Uid
        {
            /// <summary>
            /// Lentgh of uid (can be 4, 7 or 10 bytes length)
            /// </summary>
            public UidType UidType { get; set; }

            /// <summary>
            /// Contain uid of card (can be 4, 7 or 10 bytes length)
            /// </summary>
            public byte[] UidBytes { get; set; }

            /// <summary>
            /// Sak which contains usefull informations
            /// </summary>
            public byte Sak { get; set; }

            /// <summary>
            /// Get type of card
            /// </summary>
            /// <returns>Type of card</returns>
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
