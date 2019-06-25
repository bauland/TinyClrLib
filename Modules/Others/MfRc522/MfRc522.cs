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
        private GpioPin _resetPin;
        private GpioPin _irqPin; // Can be null
        private SpiDevice _spi;

        public MfRc522(string spiBus, int resetPin, int csPin, int irqPin = -1)
        {

            var gpioCtl = GpioController.GetDefault();

            _resetPin = gpioCtl.OpenPin(resetPin);
            _resetPin.SetDriveMode(GpioPinDriveMode.Output);
            _resetPin.Write(GpioPinValue.High);

            if (irqPin != -1)
            {
                _irqPin = gpioCtl.OpenPin(irqPin);
                _irqPin.SetDriveMode(GpioPinDriveMode.Input);
                _irqPin.ValueChanged += _irqPin_ValueChanged;
            }

            var settings = new SpiConnectionSettings()
            {
                ChipSelectActiveState = false,
                ChipSelectLine = csPin,
                ChipSelectType = SpiChipSelectType.Gpio,
                ClockFrequency = 2_000_000,
                DataBitLength = 8,
                Mode = SpiMode.Mode0
            };

            _spi = SpiController.FromName(spiBus).GetDevice(settings);

            HardReset();
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            // Force 100% Modulation
            WriteRegister(Register.TxASK, 0x40);

            // Set CRC to ?
            WriteRegister(Register.Mode, 0x3D);

            EnableAntennaOn();
        }

        private void WriteRegister(byte register, byte data)
        {
            throw new System.NotImplementedException();
        }

        private void EnableAntennaOn()
        {
            throw new System.NotImplementedException();
        }

        private void HardReset()
        {
            _resetPin.Write(GpioPinValue.Low);
            Thread.Sleep(1);
            _resetPin.Write(GpioPinValue.High);
            Thread.Sleep(1);
        }

        private void _irqPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
