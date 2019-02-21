using System;
using System.Drawing;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Drawing;

namespace Bauland.Adafruit
{
    /// <summary>
    /// Wrapper for manage drawing on screen
    /// </summary>
    public sealed class DrawTarget : IDrawTarget
    {
        private readonly DisplayController _parent;
        private readonly byte[] _buffer;

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="parent">Get controller to access device</param>
        public DrawTarget(DisplayController parent)
        {
            _parent = parent;

            Width = parent.ActiveConfiguration.Width;
            Height = parent.ActiveConfiguration.Height;

            _buffer = new byte[Width * Height * 2];
        }

        /// <summary>
        /// Get width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Get height
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Read data of device
        /// </summary>
        /// <returns>return buffer data</returns>
        public byte[] GetData() => _buffer;

        /// <summary>
        /// Clear screen with a color (only black is supported)
        /// </summary>
        /// <param name="color">color of screen</param>
        public void Clear(Color color) => Array.Clear(_buffer, 0, _buffer.Length);

        /// <summary>
        /// Send data buffer to screen
        /// </summary>
        public void Flush() => _parent.DrawBuffer(0, 0, Width, Height, _buffer, 0);

        /// <summary>
        /// Set color of one pixel
        /// </summary>
        /// <param name="x">x coordinate of point</param>
        /// <param name="y">y coordinate of point</param>
        /// <param name="color">Color of pixel</param>
        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;

            var idx = (y * Width + x) * 2;
            var clr = color.ToArgb();
            var red = (clr & 0b0000_0000_1111_1111_0000_0000_0000_0000) >> 16;
            var green = (clr & 0b0000_0000_0000_0000_1111_1111_0000_0000) >> 8;
            var blue = (clr & 0b0000_0000_0000_0000_0000_0000_1111_1111) >> 0;

            _buffer[idx] = (byte)((red & 0b1111_1000) | ((green & 0b1110_0000) >> 5));
            _buffer[idx + 1] = (byte)(((green & 0b0001_1100) << 3) | ((blue & 0b1111_1000) >> 3));
        }

        /// <summary>
        /// Get Pixel color
        /// </summary>
        /// <param name="x">x coordinate of point</param>
        /// <param name="y">y coordinate of point</param>
        /// <returns>Color of pixel</returns>
        /// <exception cref="NotImplementedException">Always return this exception</exception>
        public Color GetPixel(int x, int y)
        {
            throw new NotImplementedException();
        }
    }
}
