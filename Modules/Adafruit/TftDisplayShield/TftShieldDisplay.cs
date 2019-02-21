// ReSharper disable CommentTypo

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Global

using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display.Provider;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
namespace Bauland.Adafruit
{
    /// <summary>
    /// Wrapper for Adafruit TftDisplayShield
    /// </summary>
    public class TftDisplayShield : SeeSaw
    {
        /// <summary>
        /// delegate for buttons events
        /// </summary>
        /// <param name="sender">shield which fires the event</param>
        /// <param name="button">button which is pressed or released</param>
        public delegate void EventHandler(TftDisplayShield sender, int button);
        
        /// <summary>
        /// Event raised when a button is pressed
        /// </summary>
        public event EventHandler OnButtonPressed;
        
        /// <summary>
        /// Event raised when a button is released
        /// </summary>
        public event EventHandler OnButtonReleased;

        /// <summary>
        /// Constant to set backlight to the max intensity
        /// </summary>
        public const ushort BackLightOn = 0xffff;

        /// <summary>
        /// Constant to turn off backlight
        /// </summary>
        public const ushort BackLightOff = 0x0000;

        private const byte ResetPin = 0x03;
        private const byte Address = 0x2e;

        private readonly ST7735Controller _st7735Controller;
        private readonly bool _bgrPanel;
        private Orientation _orientation;

        private readonly byte[] _cmd = new byte[5];
        private int _read;
        private bool _bOldState, _bNewState;
        private int _state;
        private int _oldState;

        /// <summary>
        /// Definitions of buttons of shield
        /// </summary>
        public static class Button
        {
            /// <summary>
            /// Stick up button
            /// </summary>
            public const int Up = 1 << 5;

            /// <summary>
            /// Stick down button
            /// </summary>
            public const int Down = 1 << 8;

            /// <summary>
            /// Stick left button
            /// </summary>
            public const int Left = 1 << 6;

            /// <summary>
            /// Stick right button
            /// </summary>
            public const int Right = 1 << 9;

            /// <summary>
            /// Stick click button
            /// </summary>
            public const int Click = 1 << 7;

            /// <summary>
            /// A button
            /// </summary>
            public const int A = 1 << 10;

            /// <summary>
            /// B button
            /// </summary>
            public const int B = 1 << 11;

            /// <summary>
            /// C button
            /// </summary>
            public const int C = 1 << 14;

            /// <summary>
            /// A constant to define all buttons
            /// </summary>
            public const int All = (Up | Down | Left | Right | Click | A | B | C);


            /// <summary>
            /// Get a string which represents the button identifier
            /// </summary>
            /// <param name="button">button identifier</param>
            /// <returns>string which reprensents button</returns>
            /// <exception cref="ArgumentException">thrown when id is invalid</exception>
            public static string ToString(int button)
            {
                switch (button)
                {
                    case A:
                        return "A";
                    case B:
                        return "B";
                    case C:
                        return "C";
                    case Up:
                        return "Up";
                    case Down:
                        return "Down";
                    case Left:
                        return "Left";
                    case Right:
                        return "Right";
                    case Click:
                        return "Click";
                    default:
                        throw new ArgumentException("Parameter is not a valid ID of button.", nameof(button));
                }
            }
        }

        /// <summary>
        /// Orientation of the display
        /// </summary>
        public enum Orientation
        {
            /// <summary>
            /// Lanscape mode
            /// </summary>
            Landscape,

            /// <summary>
            /// Portrait mode
            /// </summary>
            Portrait,

            /// <summary>
            /// Reverse landscape mode
            /// </summary>
            ReverseLandscape,

            /// <summary>
            /// Reverse portrait mode
            /// </summary>
            ReversePortrait
        }

        /// <summary>
        /// Retrieve buttons state of shield
        /// </summary>
        /// <param name="raiseEvents">if true, events of buttons are fired</param>
        /// <returns>returns buttons state</returns>
        public int ReadButtons(bool raiseEvents = true)
        {
            if (!raiseEvents)
                return DigitalReadBulk(Button.All);
            _read = DigitalReadBulk(Button.All);
            _state = _read ^ Button.All;
            ProcessButtons();
            _oldState = _state;
            return _read;
        }

        private void ProcessButtons()
        {
            CheckButton(Button.A);
            CheckButton(Button.B);
            CheckButton(Button.C);

            CheckButton(Button.Up);
            CheckButton(Button.Down);
            CheckButton(Button.Left);
            CheckButton(Button.Right);
            CheckButton(Button.Click);
        }

        private void CheckButton(int buttonId)
        {
            _bOldState = (_oldState & buttonId) == buttonId;
            _bNewState = (_state & buttonId) == buttonId;
            if (!_bOldState && _bNewState)
                OnButtonPressed?.Invoke(this, buttonId);
            if (_bOldState && !_bNewState)
                OnButtonReleased?.Invoke(this, buttonId);
        }

        /// <summary>
        /// Constructor of TftDisplayShield
        /// </summary>
        /// <param name="i2CBus">Id of I2C bus</param>
        /// <param name="spiBus">Id of Spi bus</param>
        /// <param name="pinChipSelect">Id of ChipSelect pin (D10)</param>
        /// <param name="pinDataCommand">Id of control pin (D8)</param>
        /// <param name="bgrPanel">if true display is BGR panel, else it is RGB panel</param>
        public TftDisplayShield(string i2CBus, string spiBus, int pinChipSelect, int pinDataCommand, bool bgrPanel = false) : base(i2CBus, Address)
        {
            _bgrPanel = bgrPanel;
            PinMode(ResetPin, GpioPinDriveMode.Output);
            PinModeBulk(Button.All, GpioPinDriveMode.InputPullUp);
            SetBackLight(BackLightOff);
            ResetTft();
            var dc = GpioController.GetDefault().OpenPin(pinDataCommand);
            dc.SetDriveMode(GpioPinDriveMode.Output);
            _st7735Controller = new ST7735Controller(SpiController.FromName(spiBus).GetDevice(ST7735Controller.GetConnectionSettings(SpiChipSelectType.Gpio, pinChipSelect)), dc);
            SetOrientation(Orientation.Landscape);
            _oldState = ReadButtons() ^ Button.All;
        }

        /// <summary>
        /// Retrieve true height of device (depends on Orientation)
        /// </summary>
        public int Height
        {
            get
            {
                switch (_orientation)
                {
                    case Orientation.Landscape:
                    case Orientation.ReverseLandscape:
                        return _st7735Controller.Width;
                    case Orientation.Portrait:
                    case Orientation.ReversePortrait:
                        return _st7735Controller.Height;
                    default:
                        return -1;
                }
            }
        }

        /// <summary>
        /// Retrieve true width of device (depends on Orientation)
        /// </summary>
        public int Width
        {
            get
            {
                switch (_orientation)
                {
                    case Orientation.Landscape:
                    case Orientation.ReverseLandscape:
                        return _st7735Controller.Height;
                    case Orientation.ReversePortrait:
                    case Orientation.Portrait:
                        return _st7735Controller.Width;
                    default:
                        return -1;
                }
            }
        }

        /// <summary>
        /// Set orientation of display (essentially for text)
        /// </summary>
        /// <param name="orientation">orientation to be set</param>
        public void SetOrientation(Orientation orientation)
        {
            _orientation = orientation;
            switch (orientation)
            {
                case Orientation.Landscape:
                    _st7735Controller.SetDataAccessControl(true, true, false, _bgrPanel);
                    break;
                case Orientation.ReverseLandscape:
                    _st7735Controller.SetDataAccessControl(true, false, true, _bgrPanel);
                    break;
                case Orientation.Portrait:
                    _st7735Controller.SetDataAccessControl(false, true, true, _bgrPanel);
                    break;
                case Orientation.ReversePortrait:
                    _st7735Controller.SetDataAccessControl(false, false, false, _bgrPanel);
                    break;
            }
        }

        /// <summary>
        /// Get IDisplayControllerProvider to access GDI functions
        /// </summary>
        public IDisplayControllerProvider GetDisplayControllerProvider => _st7735Controller;

        /// <summary>
        /// Set intensity of backlight
        /// </summary>
        /// <param name="intensity">intensity to be set (usually a fraction of BackLightOn)</param>
        public void SetBackLight(ushort intensity)
        {
            _cmd[2] = 0x00;
            _cmd[3] = (byte)(intensity >> 8);
            _cmd[4] = (byte)(intensity);
            Write(SeeSawBaseAddress.Timer, Timer.Pwm, _cmd);
        }

        /// <summary>
        /// Reset tft display
        /// </summary>
        public void ResetTft()
        {
            DigitalWrite(ResetPin, false);
            Thread.Sleep(50);
            DigitalWrite(ResetPin, true);
            Thread.Sleep(200);
        }
    }
}
