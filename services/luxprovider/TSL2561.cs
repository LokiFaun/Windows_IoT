using System;
using Windows.Devices.I2c;

namespace luxprovider
{
    public sealed class TSL2561
    {
        private const int COMMAND = 0x80;
        private const int COMMAND_CLEAR = 0xC0;
        private const byte HIGH_GAIN = 0x10;
        private const byte INTEGRATION_TIME_SLOW = 0x02;
        private const byte POWER_DOWN = 0x00;
        private const byte POWER_UP = 0x03;
        private const int REG_CONTROL = 0x00;
        private const int REG_DATA_0 = 0x0C;
        private const int REG_DATA_1 = 0x0E;
        private const int REG_ID = 0x0A;
        private const int REG_INTCTL = 0x06;
        private const int REG_THRESH_H = 0x04;
        private const int REG_THRESH_L = 0x02;
        private const int REG_TIMING = 0x01;

        private I2cDevice m_Device;

        public TSL2561(I2cDevice device)
        {
            m_Device = device;
        }

        public static int ADDRESS { get; } = 0x39;
        public static int ADDRESS_0 { get; } = 0x29;
        public static int ADDRESS_1 { get; } = 0x49;
        public static byte FAST_TIMING { get; } = 0;
        public static byte MIDDLE_TIMING { get; } = 1;
        public static byte SLOW_TIMING { get; } = 2;

        public uint[] GetData()
        {
            var data = new uint[]
            {
                Read16(REG_DATA_0),
                Read16(REG_DATA_1)
            };
            return data;
        }

        public byte GetId()
        {
            return Read8(REG_ID);
        }

        public double GetLux(bool gain, uint ms, uint ch0, uint ch1)
        {
            if (ch0 == 0xFFFF || ch1 == 0xFFFF)
            {
                return 0xFFFF;
            }
            double d0 = ch0;
            double d1 = ch1;
            double ratio = d1 / d0;

            d0 *= (402.0 / ms);
            d1 *= (402.0 / ms);

            if (!gain)
            {
                d0 *= HIGH_GAIN;
                d1 *= HIGH_GAIN;
            }

            var lux = 0.0;
            if (ratio <= 0.5)
            {
                lux = 0.0304 * d0 - 0.062 * d0 * Math.Pow(ratio, 1.4);
            }
            else if (ratio <= 0.61)
            {
                lux = 0.0224 * d0 - 0.031 * d1;
            }
            else if (ratio <= 0.80)
            {
                lux = 0.0128 * d0 - 0.0153 * d1;
            }
            else if (ratio <= 1.30)
            {
                lux = 0.00146 * d0 - 0.00112 * d1;
            }
            return lux;
        }

        public void PowerDown()
        {
            Write8(REG_CONTROL, POWER_DOWN);
        }

        public void PowerUp()
        {
            Write8(REG_CONTROL, POWER_UP);
        }

        public int SetTiming(bool gain, byte time)
        {
            var ms = 0;
            switch (time)
            {
                case 0:
                    ms = 14;
                    break;

                case 1:
                    ms = 101;
                    break;

                case 2:
                    ms = 402;
                    break;

                default:
                    ms = 0;
                    break;
            }
            int timing = Read8(REG_TIMING);
            if (gain)
            {
                timing |= HIGH_GAIN;
            }
            else
            {
                timing &= ~HIGH_GAIN;
            }

            timing &= ~INTEGRATION_TIME_SLOW;
            timing |= (time & INTEGRATION_TIME_SLOW);

            Write8(REG_TIMING, (byte)timing);

            return ms;
        }

        private ushort Read16(byte addr)
        {
            var address = new byte[] { (byte)(addr | COMMAND) };
            var data = new byte[2];

            m_Device.WriteRead(address, data);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            var result = BitConverter.ToUInt16(data, 0);
            return result;
        }

        private byte Read8(byte addr)
        {
            var address = new byte[] { (byte)(addr | COMMAND) };
            var data = new byte[1];

            m_Device.WriteRead(address, data);

            return data[0];
        }

        private void Write8(byte addr, byte cmd)
        {
            var command = new byte[] { (byte)(addr | COMMAND), cmd };
            m_Device.Write(command);
        }
    }
}