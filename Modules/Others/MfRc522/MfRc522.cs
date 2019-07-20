using System;
using System.Diagnostics;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using Bauland.Others.Constants.MfRc522;

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
            WriteRegister(Register.TMode, 0x80);
            WriteRegister(Register.TPrescaler, 0xA9);
            WriteRegister(Register.TReloadH, 0x06);
            WriteRegister(Register.TReloadL, 0xE8);

            // Force 100% Modulation
            WriteRegister(Register.TxASK, 0x40);

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

        private StatusCode PiccSelect(out Uid uid, byte validBits = 0)
        {
            uid = new Uid();
            bool selectDone = false;
            int bitKnown = 0;
            var uidKnown = new byte[7];
            byte[] bufferBack = null;
            ClearRegisterBit(Register.Coll, 0x80);
            while (!selectDone)
            {
                var bufferLength = bitKnown == 0 ? 2 : 9;
                var buffer = new byte[bufferLength];
                bufferBack = bitKnown == 0 ? new byte[5] : new byte[3];
                byte nvb = (byte)(bitKnown == 0 ? 0x20 : 0x70);
                buffer[0] = (byte)PiccCommand.SelCl1;
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
                    return sc; // TODO: add collision
                if (sc == StatusCode.Ok)
                {
                    if (bitKnown >= 32)
                    // We know all bits
                    {
                        selectDone = true;
                        uid.UidType = UidType.T4;
                        uid.UidBytes=new byte[4];
                        Array.Copy(buffer, 2, uid.UidBytes, 0, 4);
                        uid.Sak = bufferBack[0];
                    }
                    else
                    {
                        // All bit are known, redo loop to so SELECT
                        bitKnown = 32;
                        // Save
                        for (int i = 0; i < 4; i++) // 5 is BCC
                        {
                            uidKnown[i] = bufferBack[i];
                        }
                    }
                }
            }
            return StatusCode.Ok;
        }

        private void DisplayBuffer(byte[] buffer)
        {
#if DEBUG2            
            Debug.WriteLine("#Data:");
            Debug.WriteLine($"Length: {buffer.Length}");
            var str = "";
            for (int i = 0; i < buffer.Length; i++)
                str += $"{buffer[i]:X2} ";
            Debug.WriteLine($"{str}");
#endif
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

        private StatusCode CommunicateWithPicc(PcdCommand cmd, byte waitIrq, byte[] sendData, byte[] backData, ref byte validBits, byte rxAlign = 0, bool crcCheck = false)
        {

            byte txLastBits = validBits;
            byte bitFraming = (byte)((rxAlign << 4) + txLastBits);

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
            if ((byte)(error & 0x13) == 0x13) return StatusCode.Error;

            // Get data back from Mfrc522
            if (backData != null)
            {
                byte n = ReadRegister(Register.FifoLevel);
                if (n > backData.Length) return StatusCode.NoRoom;
                // if (n < backData.Length) return StatusCode.Error;
                ReadRegister(Register.FifoData, backData, rxAlign);

                DisplayBuffer(backData);

                validBits = (byte)(ReadRegister(Register.Control) & 0x07);
            }

            // Check collision
            if ((byte)(error & 0x08) == 0x08) return StatusCode.Collision;

            // TODO:Do CrcA validation if request
            if (crcCheck)
            {

            }
            return StatusCode.Ok;
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
            for (int i = 2000; i > 0; i--)
            {
                byte n = ReadRegister(Register.ComIrq);
                if ((n & waitIrq) != 0)
                    return StatusCode.Ok;
                //TODO: not use irq timer
                //if ((n & 0x01) == 0x01)
                //{
                //    return StatusCode.Timeout;
                //}
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

        public byte ReadRegister(Register register)
        {
            _registerWriteBuffer[0] = (byte)((byte)register | 0x80);
            _registerWriteBuffer[1] = 0x00;
            _spi.TransferFullDuplex(_registerWriteBuffer, _dummyBuffer2);
            return _dummyBuffer2[1];
        }

        private void ReadRegister(Register register, byte[] backData, byte rxAlign = 0)
        {
            if (backData == null || backData.Length == 0) return;
            byte address = (byte)((byte)register | 0x80);
            byte[] writeBuffer = new byte[backData.Length + 1];
            byte[] readBuffer = new byte[backData.Length + 1];
            if (rxAlign != 0)
            {
                // TODO: to complete
            }
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
