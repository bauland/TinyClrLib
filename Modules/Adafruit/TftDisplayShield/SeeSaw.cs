using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
// ReSharper disable CommentTypo

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

namespace Bauland.Adafruit
{
    /// <summary>
    /// Class to manage Seesaw BaseAddress
    /// </summary>
    public static class SeeSawBaseAddress
    {
        /// <summary>
        /// Status registry
        /// </summary>
        public const byte Status = 0x00;
        /// <summary>
        /// Gpio registry
        /// </summary>
        public const byte Gpio = 0x01;
        /// <summary>
        /// Timer registry
        /// </summary>
        public const byte Timer = 0x08;
    }

    /// <summary>
    /// Class to manage Seesaw Status registry
    /// </summary>
    public static class SeeSawStatus
    {
        /// <summary>
        /// Retrieve HardwareId registry
        /// </summary>
        public const byte HardwareId = 0x01;
        /// <summary>
        /// Retrieve Version registry
        /// </summary>
        public const byte Version = 0x02;
        /// <summary>
        /// Retrieve Options registry
        /// </summary>
        public const byte Options = 0x03;
        /// <summary>
        /// Retrieve Temperature registry
        /// </summary>
        public const byte Temperature = 0x04;
        /// <summary>
        /// Retrieve SoftwareReset registry
        /// </summary>
        public const byte SoftwareReset = 0x7f;
        /// <summary>
        /// Retrieve Hardware Id Code (not a registry)
        /// </summary>
        public const byte HardwareIdCode = 0x55;
    }

    /// <summary>
    /// Class to manage Seesaw Gpio registry
    /// </summary>
    public static class SeeSawGpio
    {
        /// <summary>
        /// Set Direction bit
        /// </summary>
        public const byte DirectionSetBulk = 0x02;
        /// <summary>
        /// Clear Direction bit
        /// </summary>
        public const byte DirectionClearBulk = 0x03;
        /// <summary>
        /// Access pin value
        /// </summary>
        public const byte Bulk = 0x04;
        /// <summary>
        /// Write high value
        /// </summary>
        public const byte BulkSet = 0x05;
        /// <summary>
        /// Write low value
        /// </summary>
        public const byte BulkClear = 0x06;
        /// <summary>
        /// Set Pullup/Pulldown to enable
        /// </summary>
        public const byte PullEnableSet = 0x0B;
    }

    /// <summary>
    /// Class to manage Seesaw Timer registry
    /// </summary>
    public static class Timer
    {
        /// <summary>
        /// Get Status registry for timer functions
        /// </summary>
        public const byte Status = 0x00;
        /// <summary>
        /// Access to pwm registry
        /// </summary>
        public const byte Pwm = 0x01;
        /// <summary>
        /// Access to frequency registry
        /// </summary>
        public const byte Frequency = 0x02;
    }

    /// <summary>
    /// Class to manage Seesaw device.
    /// </summary>
    public class SeeSaw
    {
        private readonly I2cDevice _i2CDevice;
        private readonly bool _created;
        private readonly byte[] _wBuffer = new byte[10];
        private readonly byte[] _rBuffer = new byte[4];
        private int _read;

        #region definitions

        #endregion

        /// <summary>
        /// Constructor of Seesaw device
        /// </summary>
        /// <param name="name">Name of i2c controller</param>
        /// <param name="address">Address of device</param>
        /// <exception cref="InvalidOperationException">Thrown if device can't be joined</exception>
        protected SeeSaw(string name, int address)
        {
            _created = false;
            if (_created) throw new InvalidOperationException("Create must be called only once, call Get() after.");

            var ctl = I2cController.FromName(name);
            _i2CDevice = ctl.GetDevice(new I2cConnectionSettings(address, I2cAddressFormat.SevenBit, I2cBusSpeed.FastMode));
            SoftwareReset();
            if (!Check()) throw new InvalidOperationException($"Component not found: {name}-{address:X}");
            _created = true;
        }

        private void SoftwareReset()
        {
            _wBuffer[2] = 0xff;
            Write(SeeSawBaseAddress.Status, SeeSawStatus.SoftwareReset, _wBuffer, 3);
            Thread.Sleep(500);
        }

        private bool Check()
        {
            var res = ReadByte(SeeSawBaseAddress.Status, SeeSawStatus.HardwareId);
            return res == SeeSawStatus.HardwareIdCode;
        }

        private byte ReadByte(byte highReg, byte lowReg)
        {
            Write(highReg, lowReg, _wBuffer, 2);
            _i2CDevice.Read(_rBuffer,0,1);
            return _rBuffer[0];
        }

        /// <summary>
        /// Write a value on pin(s)
        /// </summary>
        /// <param name="pin">Pin to write to</param>
        /// <param name="value">true for High, false for Low</param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void DigitalWrite(byte pin, bool value)
        {
            if (!_created) throw new InvalidOperationException("Device must be created first.");
            if (pin >= 32)
                DigitalWriteBulk(0u, 1u << (pin - 32), value);
            else
                DigitalWriteBulk(1u << pin, value);
        }

        private void DigitalWriteBulk(uint pinsa, uint pinsb, bool value)
        {
            _wBuffer[2] = (byte)(pinsa >> 24);
            _wBuffer[3] = (byte)(pinsa >> 16);
            _wBuffer[4] = (byte)(pinsa >> 8);
            _wBuffer[5] = (byte)(pinsa);
            _wBuffer[6] = (byte)(pinsb >> 24);
            _wBuffer[7] = (byte)(pinsb >> 16);
            _wBuffer[8] = (byte)(pinsb >> 8);
            _wBuffer[9] = (byte)(pinsb);
            Write(SeeSawBaseAddress.Gpio, value ? SeeSawGpio.BulkSet : SeeSawGpio.BulkClear, _wBuffer);
        }

        private void DigitalWriteBulk(uint pins, bool value)
        {
            _wBuffer[2] = (byte)(pins >> 24);
            _wBuffer[3] = (byte)(pins >> 16);
            _wBuffer[4] = (byte)(pins >> 8);
            _wBuffer[5] = (byte)(pins);
            Write(SeeSawBaseAddress.Gpio, value ? SeeSawGpio.BulkSet : SeeSawGpio.BulkClear, _wBuffer, 6);
        }

        /// <summary>
        /// Set mode for pin(s)
        /// </summary>
        /// <param name="pin">pin(s) to set</param>
        /// <param name="mode">Mode to set</param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void PinMode(byte pin, GpioPinDriveMode mode)
        {
            if (!_created) throw new InvalidOperationException("Device must be created first.");
            if (pin >= 32)
                PinModeBulk(0, 1u << (pin - 32), mode);
            else
                PinModeBulk(1u << pin, mode);
        }


        /// <summary>
        /// Send mode pin to seesaw controller
        /// </summary>
        /// <param name="pins">pins to set</param>
        /// <param name="mode">mode to set</param>
        protected void PinModeBulk(uint pins, GpioPinDriveMode mode)
        {
            _wBuffer[2] = (byte)(pins >> 24);
            _wBuffer[3] = (byte)(pins >> 16);
            _wBuffer[4] = (byte)(pins >> 8);
            _wBuffer[5] = (byte)(pins);
            SetPinModeBulk(mode, _wBuffer, 6);
        }

        private void PinModeBulk(uint pinsa, uint pinsb, GpioPinDriveMode mode)
        {
            _wBuffer[2] = (byte)(pinsa >> 24);
            _wBuffer[3] = (byte)(pinsa >> 16);
            _wBuffer[4] = (byte)(pinsa >> 8);
            _wBuffer[5] = (byte)(pinsa);
            _wBuffer[6] = (byte)(pinsb >> 24);
            _wBuffer[7] = (byte)(pinsb >> 16);
            _wBuffer[8] = (byte)(pinsb >> 8);
            _wBuffer[9] = (byte)(pinsb);
            SetPinModeBulk(mode, _wBuffer, 10);
        }

        private void SetPinModeBulk(GpioPinDriveMode mode, byte[] cmd, int length)
        {
            switch (mode)
            {
                case GpioPinDriveMode.Output:
                    Write(SeeSawBaseAddress.Gpio, SeeSawGpio.DirectionSetBulk, cmd, length);
                    break;
                case GpioPinDriveMode.Input:
                    Write(SeeSawBaseAddress.Gpio, SeeSawGpio.DirectionClearBulk, cmd, length);
                    break;
                case GpioPinDriveMode.InputPullUp:
                    Write(SeeSawBaseAddress.Gpio, SeeSawGpio.DirectionClearBulk, cmd, length);
                    Write(SeeSawBaseAddress.Gpio, SeeSawGpio.PullEnableSet, cmd, length);
                    Write(SeeSawBaseAddress.Gpio, SeeSawGpio.BulkSet, cmd, length);
                    break;
                case GpioPinDriveMode.InputPullDown:
                    Write(SeeSawBaseAddress.Gpio, SeeSawGpio.DirectionClearBulk, cmd, length);
                    Write(SeeSawBaseAddress.Gpio, SeeSawGpio.PullEnableSet, cmd, length);
                    Write(SeeSawBaseAddress.Gpio, SeeSawGpio.BulkClear, cmd, length);
                    break;
            }
        }

        /// <summary>
        /// Write data to a registry. Data buffer must have 2 bytes more to let write high and low registry to it.
        /// </summary>
        /// <param name="highRegistry">high value of registry</param>
        /// <param name="lowRegistry">low value of registry</param>
        /// <param name="data">buffer of data to send to registry</param>
        /// <param name="length">length of data to send (buffer can me lengther to avoid a lot of buffer)</param>
        protected void Write(byte highRegistry, byte lowRegistry, byte[] data, int length = 0)
        {
            data[0] = highRegistry;
            data[1] = lowRegistry;
            if (length == 0)
                _i2CDevice.Write(data);
            else _i2CDevice.Write(data, 0, length);
        }


        /// <summary>
        /// Read pin values
        /// </summary>
        /// <param name="pins">pin(s) to be read</param>
        /// <returns>current state of pins</returns>
        protected int DigitalReadBulk(int pins)
        {
            Read(SeeSawBaseAddress.Gpio, SeeSawGpio.Bulk, _rBuffer);
            _read = ((_rBuffer[0] << 24) | (_rBuffer[1] << 16) | _rBuffer[2] << 8 | _rBuffer[3]);
            return pins & _read;
        }

        private void Read(byte highRegistry, byte lowRegistry, byte[] data)
        {
            Write(highRegistry, lowRegistry, _wBuffer, 2);
            _i2CDevice.Read(data);
        }
    }
}
