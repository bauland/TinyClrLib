using System;
using System.Diagnostics;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using Bauland.Others.Constants.MfRc522;
// ReSharper disable TooWideLocalVariableScope

namespace Bauland.Others
{
    /// <summary>
    /// 
    /// </summary>
    public class MfRc522
    {
        private readonly GpioPin _resetPin;
        //private readonly GpioPin _irqPin;
        private readonly SpiDevice _spi;
        private readonly byte[] _registerWriteBuffer;
        private readonly byte[] _dummyBuffer2;

        public MfRc522(string spiBus, int resetPin, int csPin/*, int irqPin = -1*/)
        {
            _dummyBuffer2 = new byte[2];
            _registerWriteBuffer = new byte[2];

            var gpioCtl = GpioController.GetDefault();

            _resetPin = gpioCtl.OpenPin(resetPin);
            _resetPin.SetDriveMode(GpioPinDriveMode.Output);
            _resetPin.Write(GpioPinValue.High);

            //if (irqPin != -1)
            //{
            //    _irqPin = gpioCtl.OpenPin(irqPin);
            //    _irqPin.SetDriveMode(GpioPinDriveMode.Input);
            //    _irqPin.ValueChanged += _irqPin_ValueChanged;
            //}

            var settings = new SpiConnectionSettings()
            {
                ChipSelectActiveState = false,
                ChipSelectLine = csPin,
                ChipSelectType = SpiChipSelectType.Gpio,
                ClockFrequency = 10_000_000,
                DataBitLength = 8,
                Mode = SpiMode.Mode0
            };

            _spi = SpiController.FromName(spiBus).GetDevice(settings);

            HardReset();
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            // Set Timer for Timeout
            WriteRegister(Register.TimerMode, 0x80);
            WriteRegister(Register.TimerPrescaler, 0xA9);
            WriteRegister(Register.TimerReloadHigh, 0x06);
            WriteRegister(Register.TimerReloadLow, 0xE8);

            // Force 100% Modulation
            WriteRegister(Register.TxAsk, 0x40);

            // Set CRC to 0x6363 (iso 14443-3 6.1.6)
            WriteRegister(Register.Mode, 0x3D);

            EnableAntennaOn();
        }

        private void EnableAntennaOn()
        {
            SetRegisterBit(Register.TxControl, 0x03);
        }

        //private void EnableAntennaOff()
        //{
        //    ClearRegisterBit(Register.TxControl, 0x03);
        //}

        public bool IsNewCardPresent(byte[] bufferAtqa)
        {
            if (bufferAtqa == null || bufferAtqa.Length != 2) throw new ArgumentException("bufferAtqa must be initialized and its size must be 2.", nameof(bufferAtqa));
            StatusCode sc = PiccRequestA(bufferAtqa);
            if (sc == StatusCode.Collision || sc == StatusCode.Ok) return true;
            return false;
        }

        public Uid PiccReadCardSerial()
        {
            StatusCode sc = PiccSelect(out Uid uid);
            if (sc == StatusCode.Ok)
                return uid;
            return null;
        }

        private StatusCode PiccSelect(out Uid uid)
        {
            uid = new Uid();
            bool selectDone = false;
            int bitKnown = 0;
            var uidKnown = new byte[4];
            var tempUid = new byte[10];
            byte[] bufferBack;
            ClearRegisterBit(Register.Coll, 0x80);
            int selectCascadeLevel = 1;
            int destinationIndex;
            while (!selectDone)
            {
                var bufferLength = bitKnown == 0 ? 2 : 9;
                var buffer = new byte[bufferLength];
                bufferBack = bitKnown == 0 ? new byte[5] : new byte[3];
                byte nvb = (byte)(bitKnown == 0 ? 0x20 : 0x70);
                switch (selectCascadeLevel)
                {
                    case 1:
                        buffer[0] = (byte)PiccCommand.SelCl1;
                        uid.UidType = UidType.T4;
                        destinationIndex = 0;
                        break;
                    case 2:
                        buffer[0] = (byte)PiccCommand.SelCl2;
                        uid.UidType = UidType.T7;
                        destinationIndex = 3;
                        break;
                    case 3:
                        buffer[0] = (byte)PiccCommand.SelCl3;
                        uid.UidType = UidType.T10;
                        destinationIndex = 6;
                        break;
                    default:
                        return StatusCode.Error;
                }
                //buffer[0] = (byte)PiccCommand.SelCl1;
                buffer[1] = nvb;
                if (bitKnown != 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        buffer[i + 2] = uidKnown[i];
                    }

                    buffer[6] = (byte)(buffer[2] ^ buffer[3] ^ buffer[4] ^ buffer[5]);
                    var crcStatus = CalculateCrc(buffer, 7, buffer, 7);
                    if (crcStatus != StatusCode.Ok) return crcStatus;
                }

                DisplayBuffer(buffer);
                byte validbits = 0;
                WriteRegister(Register.BitFraming, 0);

                StatusCode sc = TransceiveData(buffer, bufferBack, ref validbits);
                if (sc != StatusCode.Ok)
                {
                    return sc;
                }

                if (sc == StatusCode.Ok)
                {
                    if (bitKnown >= 32)
                    // We know all bits
                    {

                        if (buffer[2] == 0x88) // Cascade Tag
                        {
                            // check CascadeTag with SAK
                            var check = bufferBack[0] & 0x04;
                            if (check != 0x04) return StatusCode.Error;
                            // backup uid for CascadeLevel
                            Array.Copy(buffer, 3, tempUid, destinationIndex, 3);
                            selectCascadeLevel++;

                            // Clear bit know and redo REQ with next CascadeLevel
                            bitKnown = 0;
                        }
                        else
                        {
                            selectDone = true;
                            Array.Copy(buffer, 2, tempUid, destinationIndex, 4);
                            uid.Sak = bufferBack[0];
                            var check = bufferBack[0] & 0x04;
                            if (check == 0x04) return StatusCode.Error;

                        }
                    }
                    else
                    {
                        // All bit are known, redo loop to do SELECT
                        bitKnown = 32;
                        // Save
                        for (int i = 0; i < 4; i++) // 5 is BCC
                        {
                            uidKnown[i] = bufferBack[i];
                        }
                    }
                }
            }
            // Create Uid with collected infos
            uid.UidBytes = new byte[(int)uid.UidType];
            Array.Copy(tempUid, uid.UidBytes, uid.UidBytes.Length);
            return StatusCode.Ok;
        }

        [ConditionalAttribute("MYDEBUG")]
        private void DisplayBuffer(byte[] buffer)
        {
            Debug.WriteLine("#Data:");
            Debug.WriteLine($"Length: {buffer.Length}");
            var str = "";
            for (int i = 0; i < buffer.Length; i++)
                str += $"{buffer[i]:X2} ";
            Debug.WriteLine($"{str}");
        }

        private StatusCode PiccRequestA(byte[] bufferAtqa)
        {
            return ShortFrame(PiccCommand.ReqA, bufferAtqa);
        }

        private StatusCode ShortFrame(PiccCommand cmd, byte[] bufferAtqa)
        {
            ClearRegisterBit(Register.Coll, 0x80);
            byte validBits = 7;
            StatusCode sc = TransceiveData(new[] { (byte)cmd }, bufferAtqa, ref validBits);
            if (sc != StatusCode.Ok) return sc;
            if (validBits != 0)
                return StatusCode.Error;
            return StatusCode.Ok;
        }

        private StatusCode TransceiveData(byte[] buffer, byte[] bufferBack, ref byte validBits)
        {
            byte waitIrq = 0x30;
            return CommunicateWithPicc(PcdCommand.Transceive, waitIrq, buffer, bufferBack, ref validBits);
        }

        private StatusCode CommunicateWithPicc(PcdCommand cmd, byte waitIrq, byte[] sendData, byte[] backData, ref byte validBits/*, byte rxAlign = 0, bool crcCheck = false*/)
        {

            byte txLastBits = validBits;
            byte bitFraming = txLastBits;

            WriteRegister(Register.Command, (byte)PcdCommand.Idle);
            WriteRegister(Register.ComIrq, 0x7f);
            WriteRegister(Register.FifoLevel, 0x80);
            WriteRegister(Register.FifoData, sendData);
            WriteRegister(Register.BitFraming, bitFraming);
            WriteRegister(Register.Command, (byte)cmd);

            if (cmd == PcdCommand.Transceive)
            {
                SetRegisterBit(Register.BitFraming, 0x80);
            }

            StatusCode sc = WaitForCommandComplete(waitIrq);
            if (sc == StatusCode.Timeout)
            {
                return sc;
            }

            // Stop if BufferOverflow, Parity or Protocol error
            byte error = ReadRegister(Register.Error);
            if ((byte)(error & 0x13) != 0x00) return StatusCode.Error;

            // Get data back from Mfrc522
            if (backData != null)
            {
                byte n = ReadRegister(Register.FifoLevel);
                if (n > backData.Length) return StatusCode.NoRoom;
                // if (n < backData.Length) return StatusCode.Error;
                ReadRegister(Register.FifoData, backData);

                DisplayBuffer(backData);

                validBits = (byte)(ReadRegister(Register.Control) & 0x07);
            }

            // Check collision
            if ((byte)(error & 0x08) == 0x08) return StatusCode.Collision;

            return StatusCode.Ok;
        }

        public byte[][] GetSector(Uid uid, byte sector, byte[] key, PiccCommand authenticateType = PiccCommand.AuthenticateKeyA)
        {
            if (key == null || key.Length != 6) throw new ArgumentException("Key must be a byte[] of length 6.", nameof(key));
            switch (uid.GetPiccType())
            {
                case PiccType.Mifare1K:
                    return GetMifare1KSector(uid, sector, key, authenticateType);
                //case PiccType.MifareUltralight:
                //    return GetMifareUltraLight(uid, sector, key, authenticateType);
                default:
                    throw new NotImplementedException();
            }
        }

        private byte[][] GetMifare1KSector(Uid uid, byte sector, byte[] key, PiccCommand cmd = PiccCommand.AuthenticateKeyA)
        {
            if (sector > 15) throw new ArgumentOutOfRangeException(nameof(sector), "Sector must be between 0 and 16.");
            byte numberOfBlocks = 4;
            var firstblock = sector * numberOfBlocks;
            var isTrailerBlock = true;
            byte[] buffer = new byte[18];
            byte[][] returnBuffer = new byte[4][];
            for (int i = 0; i < 4; i++)
            {
                returnBuffer[i] = new byte[16];
            }
            StatusCode sc;
            for (int i = numberOfBlocks - 1; i >= 0; i--)
            {
                var blockAddr = (byte)(firstblock + i);
                if (isTrailerBlock)
                {
                    sc = Authenticate(uid, key, blockAddr, cmd);
                    if (sc != StatusCode.Ok) throw new Exception($"Authenticate() failed:{sc}");
                }
                // Read block
                sc = MifareRead(blockAddr, buffer);
                if (sc != StatusCode.Ok) throw new Exception($"MifareRead() failed:{sc}");
                if (isTrailerBlock)
                {
                    isTrailerBlock = false;
                }
                Array.Copy(buffer, returnBuffer[i], 16);
            }
            return returnBuffer;
        }

        private StatusCode MifareRead(byte blockAddr, byte[] buffer)
        {
            byte[] cmdBuffer = new byte[4];
            if (buffer == null || buffer.Length != 18) return StatusCode.NoRoom;
            cmdBuffer[0] = (byte)PiccCommand.MifareRead;
            cmdBuffer[1] = blockAddr;
            var sc = CalculateCrc(cmdBuffer, 2, cmdBuffer, 2);
            if (sc != StatusCode.Ok) return sc;
            byte validBits = 0;

            sc = TransceiveData(cmdBuffer, buffer, ref validBits);
            if (sc != StatusCode.Ok) return sc;

            // Check CRC
            byte[] crc = new byte[2];
            sc = CalculateCrc(buffer, 16, crc, 0);
            if (sc != StatusCode.Ok) return sc;
            if (buffer[16] == crc[0] && buffer[17] == crc[1]) return StatusCode.Ok;
            return StatusCode.CrcError;
        }

        public byte[] GetAccessRights(byte[][] sector)
        {
            byte[] c = new byte[3];
            if (sector.Length != 4) throw new ArgumentOutOfRangeException(nameof(sector), "Must content 4 blocks.");
            c[0] = (byte)(sector[3][7] >> 4);
            c[1] = (byte)(sector[3][8] & 0x0f);
            c[2] = (byte)(sector[3][8] >> 4);

            return c;
        }

        public byte GetVersion()
        {
            return ReadRegister(Register.Version);
        }

        public StatusCode Halt()
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)PiccCommand.HaltA;
            buffer[1] = 0;
            byte validBits = 0;
            StatusCode sc = CalculateCrc(buffer, 2, buffer, 2);
            if (sc != StatusCode.Ok) return sc;
            sc = TransceiveData(buffer, null, ref validBits);
            if (sc == StatusCode.Timeout) return StatusCode.Ok;
            if (sc == StatusCode.Ok) return StatusCode.Error;
            return sc;
        }

        public void StopCrypto()
        {
            ClearRegisterBit(Register.Status2, 0x08);
        }

        public StatusCode Authenticate(Uid uid, byte[] key, byte blockAddress, PiccCommand cmd)
        {
            if (cmd != PiccCommand.AuthenticateKeyA && cmd != PiccCommand.AuthenticateKeyB)
                throw new ArgumentException("Must be AuthenticateA or AuthenticateB only");

            if (key.Length != 6) throw new ArgumentException("Key must have a length of 6.", nameof(key));
            byte waitIrq = 0x10;
            byte validBits = 0;
            byte[] buffer = new byte[12];
            buffer[0] = (byte)cmd;
            buffer[1] = blockAddress;
            // set key
            for (int i = 0; i < 6; i++)
            {
                buffer[i + 2] = key[i];
            }
            // set uid
            for (int i = 0; i < 4; i++)
            {
                buffer[i + 8] = uid.UidBytes[uid.UidType == UidType.T4 ? i : i + 3];
            }

            return CommunicateWithPicc(PcdCommand.MfAuthenticate, waitIrq, buffer, null, ref validBits);
        }

        /// <summary>
        /// Calculate Crc of a buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="lengthBuffer"></param>
        /// <param name="bufferBack"></param>
        /// <param name="indexBufferBack"></param>
        /// <returns></returns>
        private StatusCode CalculateCrc(byte[] buffer, int lengthBuffer, byte[] bufferBack, int indexBufferBack)
        {
            byte[] shortBuffer = new byte[lengthBuffer];
            Array.Copy(buffer, shortBuffer, lengthBuffer);
            WriteRegister(Register.Command, (byte)PcdCommand.Idle);
            WriteRegister(Register.DivIrq, 0x04);
            WriteRegister(Register.FifoLevel, 0x80);
            WriteRegister(Register.FifoData, shortBuffer);
            WriteRegister(Register.Command, (byte)PcdCommand.CalculateCrc);
            for (int i = 500; i > 0; i--)
            {
                byte n = ReadRegister(Register.DivIrq);
                if ((n & 0x04) == 0x04)
                {
                    WriteRegister(Register.Command, (byte)PcdCommand.Idle);
                    bufferBack[indexBufferBack] = ReadRegister(Register.CrcResultLow);
                    bufferBack[indexBufferBack + 1] = ReadRegister(Register.CrcResultHigh);
                    return StatusCode.Ok;
                }
            }

            return StatusCode.Timeout;
        }

        private StatusCode WaitForCommandComplete(byte waitIrq)
        {
            for (int i = 200; i > 0; i--)
            {
                byte n = ReadRegister(Register.ComIrq);
                if ((n & waitIrq) != 0)
                    return StatusCode.Ok;
                if ((n & 0x01) == 0x01)
                {
                    return StatusCode.Timeout;
                }
            }
            return StatusCode.Timeout;
        }

        private void HardReset()
        {
            _resetPin.Write(GpioPinValue.Low);
            Thread.Sleep(1);
            _resetPin.Write(GpioPinValue.High);
            Thread.Sleep(1);
        }

        #region Basic Communication functions

        private void WriteRegister(Register register, byte data)
        {
            _registerWriteBuffer[0] = (byte)register;
            _registerWriteBuffer[1] = data;
            _spi.TransferFullDuplex(_registerWriteBuffer, _dummyBuffer2);
        }

        private void WriteRegister(Register register, byte[] data)
        {
            foreach (var b in data)
            {
                WriteRegister(register, b);
            }
        }

        private byte ReadRegister(Register register)
        {
            _registerWriteBuffer[0] = (byte)((byte)register | 0x80);
            _registerWriteBuffer[1] = 0x00;
            _spi.TransferFullDuplex(_registerWriteBuffer, _dummyBuffer2);
            return _dummyBuffer2[1];
        }

        private void ReadRegister(Register register, byte[] backData)
        {
            if (backData == null || backData.Length == 0) return;
            byte address = (byte)((byte)register | 0x80);
            byte[] writeBuffer = new byte[backData.Length + 1];
            byte[] readBuffer = new byte[backData.Length + 1];

            for (int i = 0; i < backData.Length; i++) writeBuffer[i] = address;
            _spi.TransferFullDuplex(writeBuffer, readBuffer);
            Array.Copy(readBuffer, 1, backData, 0, backData.Length);
        }

        private void SetRegisterBit(Register register, byte mask)
        {
            var tmp = ReadRegister(register);
            WriteRegister(register, (byte)(tmp | mask));
        }

        private void ClearRegisterBit(Register register, byte mask)
        {
            var tmp = ReadRegister(register);
            WriteRegister(register, (byte)(tmp & ~mask));
        }
        #endregion

    }
}
