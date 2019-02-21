using System;
using System.Drawing;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Drawing;

namespace Bauland.Adafruit
{
    internal sealed class DrawTarget : IDrawTarget
    {
        private readonly DisplayController _parent;
        private readonly byte[] _buffer;

        public DrawTarget(DisplayController parent)
        {
            _parent = parent;

            Width = parent.ActiveConfiguration.Width;
            Height = parent.ActiveConfiguration.Height;

            _buffer = new byte[Width * Height * 2];
        }

        public int Width { get; }
        public int Height { get; }

        public void Dispose() { }
        public byte[] GetData() => _buffer;
        public Color GetPixel(int x, int y) => throw new NotSupportedException();

        public void Clear(Color color) => Array.Clear(_buffer, 0, _buffer.Length);

        public void Flush() => _parent.DrawBuffer(0, 0, Width, Height, _buffer, 0);

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
    }
}
