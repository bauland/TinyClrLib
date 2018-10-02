using System;
using GHIElectronics.TinyCLR.Devices.Signals;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Bauland.Grove
{
    /// <summary>
    /// Wrapper class for UltrasonicRanger Grove Module
    /// </summary>
    public class UltrasonicRanger
    {


        private readonly PulseFeedback _pulse;

        /// <summary>
        /// Use to correct measurement in linear way:  result = value * A + B (default: 0.81f)
        /// </summary>
        public float A = 0.81f;

        /// <summary>
        /// Use to correct measurement in linear way:  result = value * A + B (default: 2.11f)
        /// </summary>
        public float B = 2.11f;

        /// <summary>
        /// Constructor of HC-SR04
        /// </summary>
        /// <param name="signalPin">pin connected to trigger and echo pin</param>
        public UltrasonicRanger(int signalPin)
        {
            //_pulse = new GpioPulseReaderWriter(
            //    GpioPulseReaderWriter.Mode.EchoDuration, true, 10, signalPin);
            // Note:
            // 1 tick = 100ns
            // 100 ticks = 10us
            _pulse = new PulseFeedback(signalPin, PulseFeedbackMode.EchoDuration)
            {
                PulseLength = TimeSpan.FromTicks(100)
            };
        }

        /// <summary>
        /// Get distance in centimeters
        /// </summary>
        /// <returns>Value return is in centimeters</returns>
        public float ReadCentimeters()
        {
            return (float)(_pulse.GeneratePulse().TotalMilliseconds * 17 / 1000.0) * A + B;
        }

        /// <summary>
        /// Get distance in inches
        /// </summary>
        /// <returns>Value return is in inches</returns>
        public double ReadInches()
        {
            return ReadCentimeters() / 2.54;
        }

    }
}
