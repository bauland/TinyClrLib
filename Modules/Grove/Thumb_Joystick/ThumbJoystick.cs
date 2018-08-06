using System;
using GHIElectronics.TinyCLR.Devices.Adc;
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Bauland.Grove
{
    /// <summary>
    /// Wrapper for Grove ThumbJoystick module
    /// </summary>
    public class ThumbJoystick
    {
        private bool _bReadButtonClick;
        private bool _bReadButtonUp;
        private bool _bReadButtonDown;
        private bool _bReadButtonLeft;
        private bool _bReadButtonRight;
        private readonly AdcChannel _xChannel;
        private readonly AdcChannel _yChannel;
        private int _clickThreshold = 4000;
        private int _xMinDead = 2000, _xMaxDead = 2100, _yMinDead = 2000, _yMaxDead = 2100;

        /// <summary>
        /// Delegate for all events of Joystick
        /// </summary>
        /// <param name="joystick">Joystick instance</param>
        public delegate void EventHandler(ThumbJoystick joystick);

        /// <summary>
        /// Event raise when Joystick is clicked
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Event raise when Joystick is move up
        /// </summary>
        public event EventHandler Up;

        /// <summary>
        /// Event raise when Joystick is move down
        /// </summary>
        public event EventHandler Down;

        /// <summary>
        /// Event raise when Joystick is move left
        /// </summary>
        public event EventHandler Left;

        /// <summary>
        /// Event raise when Joystick is move right
        /// </summary>
        public event EventHandler Right;

        /// <summary>
        /// Get Y value of joystick
        /// </summary>
        public int Y { get; private set; }
        /// <summary>
        /// Get X value of joystick
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Return state of button
        /// </summary>
        public bool IsPressed => X > _clickThreshold;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="xChannelPin">Pin of adc channel to read x(yellow wire)</param>
        /// <param name="yChannelPin">Pin of adc channel to read y(white wire)</param>
        public ThumbJoystick(int xChannelPin, int yChannelPin)
        {
            SetDeadZone(2050, 2025, 50);
            _bReadButtonClick = true;
            _bReadButtonDown = true;
            _bReadButtonLeft = true;
            _bReadButtonRight = true;
            _bReadButtonUp = true;
            var adcCtl = AdcController.GetDefault();
            _xChannel = adcCtl.OpenChannel(xChannelPin);
            _yChannel = adcCtl.OpenChannel(yChannelPin);
        }

        /// <summary>
        /// Read state of Joystick
        /// </summary>
        public void Read()
        {
            X = _xChannel.ReadValue();
            Y = _yChannel.ReadValue();
            //Debug.WriteLine($"x: {X}, y: {Y}");
            _bReadButtonClick = CheckEvents(X > _clickThreshold, _bReadButtonClick, Click);
            _bReadButtonDown = CheckEvents(Y < _yMinDead, _bReadButtonDown, Down);
            _bReadButtonUp = CheckEvents(Y > _yMaxDead, _bReadButtonUp, Up);
            _bReadButtonLeft = CheckEvents(X < _xMinDead, _bReadButtonLeft, Left);
            _bReadButtonRight = CheckEvents(X > _xMaxDead, _bReadButtonRight, Right);
        }

        private bool CheckEvents(bool bCondition, bool bProtectEvent, EventHandler onEvent)
        {
            if (bCondition)
            {
                if (bProtectEvent)
                {
                    onEvent?.Invoke(this);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set zone in which joystick is not moving
        /// </summary>
        /// <param name="xMin">Lower x value (default: 2000)</param>
        /// <param name="xMax">Greater x value (default: 2100)</param>
        /// <param name="yMin">Lower y value (default: 1975)</param>
        /// <param name="yMax">Greate y value (default: 2075)</param>
        /// <exception cref="ArgumentException">Raised when lower values are greater than greater values</exception>
        public void SetDeadZone(int xMin, int xMax, int yMin, int yMax)
        {
            if ((xMin > xMax) || (yMin > yMax))
            {
                throw new ArgumentException("xMin must be lesser than xMax and yMin must be lesser than yMax");
            }

            _xMinDead = xMin;
            _yMinDead = yMin;
            _xMaxDead = xMax;
            _yMaxDead = yMax;
        }

        /// <summary>
        /// Set zone in which joystick is not moving
        /// </summary>
        /// <param name="x">x value of center point (default: 2050)</param>
        /// <param name="y">y value of center point (default: 2025)</param>
        /// <param name="radius">radius in between joystick is not moving (default: 50)</param>
        /// <exception cref="ArgumentException">Raised when radius is greater than x or y value</exception>
        public void SetDeadZone(int x, int y, int radius)
        {
            if (x < radius)
                throw new ArgumentException("Radius must be lowest to x", nameof(radius));
            if (y < radius)
                throw new ArgumentException("Radius must be lowest to y", nameof(radius));
            _xMinDead = x - radius;
            _xMaxDead = x + radius;
            _yMinDead = y - radius;
            _yMaxDead = y + radius;
        }

        /// <summary>
        /// Set the treshold beyond button is clicked 
        /// </summary>
        /// <param name="threshold">value of threshold (default: 4000)</param>
        public void SetClickThreshold(int threshold)
        {
            _clickThreshold = threshold;
        }
    }
}
