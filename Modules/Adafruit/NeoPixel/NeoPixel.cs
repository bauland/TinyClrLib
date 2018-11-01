using System;
using GHIElectronics.TinyCLR.Devices.Spi;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Bauland.Adafruit
{
    /// <summary>
    /// Wrapper for Adafruit NeoPixel modules (stick, shield, ...)
    /// </summary>
    public class NeoPixel
    {
        /// <summary>
        /// Number of DELs in chain 
        /// </summary>
        public int Size { get; }
        private readonly byte[] _buffer;
        private readonly byte[] _temp;
        private readonly SpiDevice _spi;
        /// <summary>
        /// Constructor of NeoPixel class
        /// </summary>
        /// <param name="spi">Identifier of spi bus</param>
        /// <param name="numLed">Number of DELs in chain</param>
        public NeoPixel(string spi, int numLed)
        {
            Size = numLed;
            _temp = new byte[4];
            _buffer = new byte[Size * 12];
            var settings = new SpiConnectionSettings()
            {
                ClockFrequency = 3200 * 1000,
                Mode = SpiMode.Mode1,
                DataBitLength = 8,
                ChipSelectType = SpiChipSelectType.None
            };
            var controller = SpiController.FromName(spi);
            _spi = controller.GetDevice(settings);
            Reset();
        }

        private void Reset()
        {
            for (int i = 0; i < _buffer.Length; i++)
            {
                _buffer[i] = 0x0;
            }
            Show(); // Send reset cmd

            // Initialize _buffer to 0 (0x88 values)
            for (int i = 0; i < _buffer.Length; i++)
            {
                _buffer[i] = 0x88;
            }
            Show(); // Set all Black
        }

        /// <summary>
        /// Change color of one of DEL
        /// </summary>
        /// <param name="index">Index (starting from 0) of DEL</param>
        /// <param name="r">Value of red component</param>
        /// <param name="g">Value of green component</param>
        /// <param name="b">Value of blue component</param>
        /// <exception cref="ArgumentOutOfRangeException">Could be raised if index param is out of range</exception>
        public void ChangeColor(int index, byte r, byte g, byte b)
        {
            if (index < 0 || index >= Size) throw new ArgumentOutOfRangeException(nameof(index));
            var startArray = index * 12;
            Compute(g, _temp);
            Array.Copy(_temp, 0, _buffer, startArray, 4);
            Compute(r, _temp);
            Array.Copy(_temp, 0, _buffer, startArray + 4, 4);
            Compute(b, _temp);
            Array.Copy(_temp, 0, _buffer, startArray + 8, 4);
        }

        /// <summary>
        /// Change color of all DEL
        /// </summary>
        /// <param name="r">Value of red component</param>
        /// <param name="g">Value of green component</param>
        /// <param name="b">Value of blue component</param>
        public void ChangeAllColor(byte r, byte g, byte b)
        {
            Compute(g, _temp);
            for (int index = 0; index < Size; index++)
            {
                var startArray = index * 12;
                Array.Copy(_temp, 0, _buffer, startArray, 4);
            }
            Compute(r, _temp);
            for (int index = 0; index < Size; index++)
            {
                var startArray = index * 12;
                Array.Copy(_temp, 0, _buffer, startArray + 4, 4);
            }
            Compute(b, _temp);
            for (int index = 0; index < Size; index++)
            {
                var startArray = index * 12;
                Array.Copy(_temp, 0, _buffer, startArray + 8, 4);
            }
        }

        /// <summary>
        /// Turn off one DEL
        /// </summary>
        /// <param name="index">Index (starting from 0) of DEL</param>
        public void Off(int index)
        {
            ChangeColor(index, 0, 0, 0);
        }

        /// <summary>
        /// Turn off all DELs
        /// </summary>
        public void Off()
        {
            ChangeAllColor(0, 0, 0);
        }

        private static void Compute(byte b, byte[] array)
        {
            for (byte i = 0; i < 4; i++)
            {
                array[3 - i] = (byte)(((b & (0x01 << (2 * i))) == (0x01 << (2 * i)) ? 0x0c : 0x08) | ((b & (0x01 << (2 * i + 1))) == (0x01 << (2 * i + 1)) ? 0xc0 : 0x80));
            }
        }


        /// <summary>
        /// Send buffer to DELs to display
        /// </summary>
        public void Show()
        {
            _spi.Write(_buffer);
        }
    }
}
