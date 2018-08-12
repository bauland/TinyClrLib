// ReSharper disable UnusedMember.Global
namespace Bauland.Others
{
    internal class Led
    {
        private readonly LedStrip.ColorOrder _type;
        private readonly byte[] _data;

        public byte Brightness
        {
            get => _data[0];
            set => _data[0] = (byte)(value | 0xe0);
        }

        public byte Red
        {
            get
            {
                if (_type == LedStrip.ColorOrder.Bgr)
                    return _data[3];
                return _data[1];
            }
            set
            {
                if (_type == LedStrip.ColorOrder.Bgr)
                    _data[3] = value;
                else
                    _data[1] = value;
            }
        }

        public byte Green
        {
            get => _data[2];
            set => _data[2] = value;
        }

        public byte Blue
        {
            get
            {
                if (_type == LedStrip.ColorOrder.Bgr)
                    return _data[1];
                return _data[3];
            }
            set
            {
                if (_type == LedStrip.ColorOrder.Bgr)
                    _data[1] = value;
                else
                    _data[3] = value;
            }
        }

        public Led(LedStrip.ColorOrder type)
        {
            _type = type;
            _data = new byte[4];
        }

        public byte[] GetData()
        {
            return _data;
        }
    }
}
