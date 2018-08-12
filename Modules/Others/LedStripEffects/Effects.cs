using System;
using System.Threading;
// ReSharper disable UnusedMember.Global

namespace Bauland.Others
{
    /// <summary>
    /// Add effects to LedStrip
    /// </summary>
    public static class Effects
    {
        private static readonly Random Random;

        static Effects()
        {
            Random = new Random((int)DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// Fade in strip with a color
        /// </summary>
        /// <param name="strip">Strip of LEDs</param>
        /// <param name="delay">Delay of effect</param>
        /// <param name="brightness">Brightness of LEDs</param>
        /// <param name="red">Value of red for LEDs color</param>
        /// <param name="green">Value of green for LEDs color</param>
        /// <param name="blue">Value of blue for LEDs color</param>
        public static void FadeIn(this LedStrip strip, int delay, byte brightness, byte red, byte green, byte blue)
        {
            for (int k = 0; k < 256; k++)
            {
                strip.SetAll(brightness, (byte)(red * k / 255), (byte)(green * k / 255), (byte)(blue * k / 255));
                strip.Show();
                Thread.Sleep(delay);
            }
        }
        /// <summary>
        /// Fade out strip with a color
        /// </summary>
        /// <param name="strip">Strip of LEDs</param>
        /// <param name="delay">Delay of effect</param>
        /// <param name="brightness">Brightness of LEDs</param>
        /// <param name="red">Value of red for LEDs color</param>
        /// <param name="green">Value of green for LEDs color</param>
        /// <param name="blue">Value of blue for LEDs color</param>
        public static void FadeOut(this LedStrip strip, int delay, byte brightness, byte red, byte green, byte blue)
        {
            for (int k = 255; k > -1; k--)
            {
                strip.SetAll(brightness, (byte)(red * k / 255), (byte)(green * k / 255), (byte)(blue * k / 255));
                strip.Show();
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// Cycle color of strip by rainbow
        /// </summary>
        /// <param name="strip">Strip of LEDs</param>
        /// <param name="brightness">Brightness of LEDs</param>
        /// <param name="delay">Delay of effect</param>
        public static void RainbowWheel(this LedStrip strip, int delay, byte brightness)
        {
            // ReSharper disable once TooWideLocalVariableScope
            byte[] color;
            for (int j = 0; j < 256; j++)
            {
                for (int i = 0; i < strip.Size; i++)
                {
                    color = Wheel((byte)(((i * 256 / strip.Size) + j) & 0xff));
                    strip.SetPixel(i, brightness, color[0], color[1], color[2]);
                }

                strip.Show();
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// Turn on a LED after each other
        /// </summary>
        /// <param name="strip">Strip of LEDs</param>
        /// <param name="delay">Delay of effect</param>
        /// <param name="brightness">Brightness of LEDs</param>
        /// <param name="red">Value of red for LEDs color</param>
        /// <param name="green">Value of green for LEDs color</param>
        /// <param name="blue">Value of blue for LEDs color</param>
        public static void Wipe(this LedStrip strip, int delay, byte brightness, byte red, byte green, byte blue)
        {
            for (int i = 0; i < strip.Size; i++)
            {
                strip.SetPixel(i, brightness, red, green, blue);
                strip.Show();
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// Turn on a third of LEDs, turn off and move one step
        /// </summary>
        /// <param name="strip">Strip of LEDs</param>
        /// <param name="delay">Delay of effect</param>
        /// <param name="brightness">Brightness of LEDs</param>
        /// <param name="red">Value of red for LEDs color</param>
        /// <param name="green">Value of green for LEDs color</param>
        /// <param name="blue">Value of blue for LEDs color</param>
        public static void RunningLights(this LedStrip strip, int delay, byte brightness, byte red, byte green, byte blue)
        {
            int position = 0;
            for (int j = 0; j < strip.Size * 2; j++)
            {
                position++;
                for (int i = 0; i < strip.Size; i++)
                {
                    strip.SetPixel(i, brightness,
                        (byte)((Math.Sin(i + position) * 127 + 128) / 255 * red),
                        (byte)((Math.Sin(i + position) * 127 + 128) / 255 * green),
                        (byte)((Math.Sin(i + position) * 127 + 128) / 255 * blue));
                }
                strip.Show();
                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// Turn on a third of LEDs, turn off and move one step
        /// </summary>
        /// <param name="strip">Strip of LEDs</param>
        /// <param name="delay">Delay of effect</param>
        /// <param name="brightness">Brightness of LEDs</param>
        /// <param name="red">Value of red for LEDs color</param>
        /// <param name="green">Value of green for LEDs color</param>
        /// <param name="blue">Value of blue for LEDs color</param>
        /// <param name="count">Number of times to sparkle</param>
        public static void Sparkle(this LedStrip strip, int delay, byte brightness, byte red, byte green, byte blue, uint count)
        {
            for (uint i = 0; i < count; i++)
            {
                var index = Random.Next(strip.Size);
                strip.SetPixel(index, brightness, red, green, blue);
                strip.Show();
                Thread.Sleep(delay);
                strip.SetPixel(index, 0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Turn on a third of LEDs, turn off and move one step
        /// </summary>
        /// <param name="strip">Strip of LEDs</param>
        /// <param name="delay">Delay of effect</param>
        /// <param name="brightness">Brightness of LEDs</param>
        /// <param name="red">Value of red for LEDs color</param>
        /// <param name="green">Value of green for LEDs color</param>
        /// <param name="blue">Value of blue for LEDs color</param>
        /// <param name="count"></param>
        public static void TheaterChase(this LedStrip strip, int delay, byte brightness, byte red, byte green, byte blue, int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), "count must be greater than 0.");
            strip.Clear();
            for (int j = 0; j < count; j++)
            {
                for (int q = 0; q < 3; q++)
                {
                    for (int i = 0; i < strip.Size; i = i + 3)
                    {
                        if (i + q < strip.Size)
                            strip.SetPixel(i + q, brightness, red, green, blue);
                    }
                    strip.Show();

                    Thread.Sleep(delay);

                    for (int i = 0; i < strip.Size; i = i + 3)
                    {
                        if (i + q < strip.Size)
                            strip.SetPixel(i + q, brightness, 0, 0, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Turn on random led with random color
        /// </summary>
        /// <param name="strip">Strip of LEDs</param>
        /// <param name="delay">Delay of effect</param>
        /// <param name="brightness">Brightness of LEDs</param>
        /// <param name="count">Number of LEDs to turn on</param>
        public static void TwinkleRandom(this LedStrip strip, int delay, byte brightness, int count)
        {
            strip.SetAll(brightness, 0, 0, 0);
            for (int i = 0; i < count; i++)
            {
                strip.SetPixel(Random.Next(strip.Size), brightness, (byte)Random.Next(255),
                    (byte)Random.Next(255), (byte)Random.Next(255));
                strip.Show();
                Thread.Sleep(delay);
            }

            Thread.Sleep(delay);
        }
        private static byte[] Wheel(byte l)
        {
            byte[] color = new byte[3];
            if (l < 85)
            {
                color[0] = (byte)(l * 3);
                color[1] = (byte)(255 - l * 3);
                color[2] = 0;
            }
            else if (l < 170)
            {
                l -= 85;
                color[0] = (byte)(255 - l * 3);
                color[1] = 0;
                color[2] = (byte)(l * 3);
            }
            else
            {
                l -= 170;
                color[0] = 0;
                color[1] = (byte)(l * 3);
                color[2] = (byte)(255 - l * 3);
            }
            return color;
        }

    }
}
