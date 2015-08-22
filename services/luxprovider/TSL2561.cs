using System;
using Windows.Devices.I2c;

namespace luxprovider
{
    /// <summary>
    /// Representation of TSL2561 sensor
    /// </summary>
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
        private const int LUX_SCALE = 15;
        private const int RATIO_SCALE = 9;
        private const int CH_SCALE = 10;
        private const int CH0_SCALE = 0x7517;
        private const int CH1_SCALE = 0x0fe7;
        private const byte FAST_TIMING = 0;
        private const byte MIDDLE_TIMING = 1;
        private const byte SLOW_TIMING = 2;
        private const int K1C = 0x0043;
        private const int B1C = 0x0204;
        private const int M1C = 0x01ad;
        private const int K2C = 0x0085;
        private const int B2C = 0x0228;
        private const int M2C = 0x02c1;
        private const int K3C = 0x00c8;
        private const int B3C = 0x0253;
        private const int M3C = 0x0363;
        private const int K4C = 0x010a;
        private const int B4C = 0x0282;
        private const int M4C = 0x03df;
        private const int K5C = 0x014d;
        private const int B5C = 0x0177;
        private const int M5C = 0x01dd;
        private const int K6C = 0x019a;
        private const int B6C = 0x0101;
        private const int M6C = 0x0127;
        private const int K7C = 0x029a;
        private const int B7C = 0x0037;
        private const int M7C = 0x002b;
        private const int K8C = 0x029a;
        private const int B8C = 0x0000;
        private const int M8C = 0x0000;

        private I2cDevice m_Device;

        /// <summary>
        /// Initializes the sensor
        /// </summary>
        /// <param name="device">The I2C device for communication with the sensor</param>
        public TSL2561(I2cDevice device)
        {
            m_Device = device;
        }

        public static int Address { get; } = 0x39;
        public static int Address_0 { get; } = 0x29;
        public static int Address_1 { get; } = 0x49;
        public static byte FastTiming { get; } = 0;
        public static byte MiddleTiming { get; } = 1;
        public static byte SlowTiming { get; } = 2;

        /// <summary>
        /// Reads data from data_0 and data_1 registers
        /// </summary>
        /// <returns>Contents of data_0 and data_1 registers</returns>
        public uint[] GetData()
        {
            var data = new uint[]
            {
                Read16(REG_DATA_0),
                Read16(REG_DATA_1)
            };
            return data;
        }

        /// <summary>
        /// Reads the sensor ID
        /// </summary>
        /// <returns>The sensor ID</returns>
        public byte GetId()
        {
            return Read8(REG_ID);
        }

        /// <summary>
        /// Converts channel_0 and channel_1 data to LUX values using gain and ms
        /// </summary>
        /// <param name="gain">Specifies if high or low gain is used</param>
        /// <param name="timing">Specifies the timing used</param>
        /// <param name="ch0">Channel_0 data</param>
        /// <param name="ch1">Channel_1 data</param>
        /// <returns>Lux representation of channel_0 and channel_1 data</returns>
        public ulong GetLux(bool gain, uint timing, uint ch0, uint ch1)
        {
            if (ch0 == 0xFFFF || ch1 == 0xFFFF)
            {
                return 0xFFFF;
            }
            ulong scale = 0;

            // normalize to timing
            switch (timing)
            {
                case FAST_TIMING:
                    scale = CH0_SCALE;
                    break;

                case MIDDLE_TIMING:
                    scale = CH1_SCALE;
                    break;

                default:
                    scale = (1 << CH_SCALE);
                    break;
            }

            // normalize to gain
            if (!gain)
            {
                scale *= HIGH_GAIN;
            }

            ulong d0 = (ch0 * scale) >> CH_SCALE;
            ulong d1 = (ch1 * scale) >> CH_SCALE;
            ulong ratio = 0;
            if (d0 != 0)
            {
                ratio = (d1 << (RATIO_SCALE + 1)) / d0;
            }
            ratio = (ratio + 1) >> 1;

            // calculate lux according to datasheet
            var lux = 0UL;
            if (ratio <= K1C)
            {
                lux = B1C * d0 - d1 * M1C;
            }
            else if (ratio <= K2C)
            {
                lux = B2C * d0 - M2C * d1;
            }
            else if (ratio <= K3C)
            {
                lux = B3C * d0 - M3C * d1;
            }
            else if (ratio <= K4C)
            {
                lux = B4C * d0 - M4C * d1;
            }
            else if (ratio <= K5C)
            {
                lux = B5C * d0 - M5C * d1;
            }
            else if (ratio <= K6C)
            {
                lux = B6C * d0 - M6C * d1;
            }
            else if (ratio <= K7C)
            {
                lux = B7C * d0 - M7C * d1;
            }
            else if (ratio > K8C)
            {
                lux = B8C * d0 - M8C * d1;
            }
            lux = lux > 0 ? lux : 0;
            lux += (1 << (LUX_SCALE - 1));
            lux = lux >> LUX_SCALE;

            return lux;
        }

        /// <summary>
        /// Power down the sensor
        /// </summary>
        public void PowerDown()
        {
            Write8(REG_CONTROL, POWER_DOWN);
        }

        /// <summary>
        /// Power up the sensor
        /// </summary>
        public void PowerUp()
        {
            Write8(REG_CONTROL, POWER_UP);
        }

        /// <summary>
        /// Set the timing to use for lux measurement
        /// </summary>
        /// <param name="gain">Specifies if high or low gain shall be used</param>
        /// <param name="time">Specifies the timing: 0 => 14ms, 1 => 101ms, 2 => 402ms</param>
        /// <returns></returns>
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
                    throw new ArgumentOutOfRangeException("time", "Possible values: 0, 1, 2");
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

        /// <summary>
        /// Reads 2 bytes from the specified address
        /// </summary>
        /// <param name="addr">Register to read from</param>
        /// <returns>2 byte register value</returns>
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

        /// <summary>
        /// Read 1 byte form the specified address
        /// </summary>
        /// <param name="addr">Register to read from</param>
        /// <returns>1 byte register value</returns>
        private byte Read8(byte addr)
        {
            var address = new byte[] { (byte)(addr | COMMAND) };
            var data = new byte[1];

            m_Device.WriteRead(address, data);

            return data[0];
        }

        /// <summary>
        /// Writes 1 byte to the specified address
        /// </summary>
        /// <param name="addr">Register to write to</param>
        /// <param name="cmd">Command to write</param>
        private void Write8(byte addr, byte cmd)
        {
            var command = new byte[] { (byte)(addr | COMMAND), cmd };
            m_Device.Write(command);
        }
    }
}