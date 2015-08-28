using System;
using Windows.Devices.I2c;

namespace luxprovider
{
    /// <summary>
    /// Representation of TSL2561 sensor
    /// </summary>
    public sealed class TSL2561
    {
        private const int TSL2561_VISIBLE = 2;                   // channel 0 - channel 1
        private const int TSL2561_INFRARED = 1;                  // channel 1
        private const int TSL2561_FULLSPECTRUM = 0;              // channel 0

        // I2C address options
        private const int TSL2561_ADDR_LOW = (0x29);
        private const int TSL2561_ADDR_FLOAT = (0x39);    // Default address = (pin left; floating)
        private const int TSL2561_ADDR_HIGH = (0x49);

        // Lux calculations differ slightly for CS package

        private const int TSL2561_COMMAND_BIT = (0x80);    // Must be 1
        private const int TSL2561_CLEAR_BIT = (0x40);    // Clears any pending interrupt = (write 1; to clear)
        private const int TSL2561_WORD_BIT = (0x20);    // 1 = read/write word = (rather than; byte)
        private const int TSL2561_BLOCK_BIT = (0x10);    // 1 = using block read/write

        private const int TSL2561_CONTROL_POWERON = (0x03);
        private const int TSL2561_CONTROL_POWEROFF = (0x00);

        private const int TSL2561_LUX_LUXSCALE = (14);      // Scale by 2^14
        private const int TSL2561_LUX_RATIOSCALE = (9);       // Scale ratio by 2^9
        private const int TSL2561_LUX_CHSCALE = (10);      // Scale channel values by 2^10
        private const int TSL2561_LUX_CHSCALE_TINT0 = (0x7517);  // 322/11 * 2^TSL2561_LUX_CHSCALE
        private const int TSL2561_LUX_CHSCALE_TINT1 = (0x0FE7);  // 322/81 * 2^TSL2561_LUX_CHSCALE

        // CS package values
        private const int TSL2561_LUX_K1C = (0x0043);  // 0.130 * 2^RATIO_SCALE
        private const int TSL2561_LUX_B1C = (0x0204);  // 0.0315 * 2^LUX_SCALE
        private const int TSL2561_LUX_M1C = (0x01ad);  // 0.0262 * 2^LUX_SCALE
        private const int TSL2561_LUX_K2C = (0x0085);  // 0.260 * 2^RATIO_SCALE
        private const int TSL2561_LUX_B2C = (0x0228);  // 0.0337 * 2^LUX_SCALE
        private const int TSL2561_LUX_M2C = (0x02c1);  // 0.0430 * 2^LUX_SCALE
        private const int TSL2561_LUX_K3C = (0x00c8);  // 0.390 * 2^RATIO_SCALE
        private const int TSL2561_LUX_B3C = (0x0253);  // 0.0363 * 2^LUX_SCALE
        private const int TSL2561_LUX_M3C = (0x0363);  // 0.0529 * 2^LUX_SCALE
        private const int TSL2561_LUX_K4C = (0x010a);  // 0.520 * 2^RATIO_SCALE
        private const int TSL2561_LUX_B4C = (0x0282);  // 0.0392 * 2^LUX_SCALE
        private const int TSL2561_LUX_M4C = (0x03df);  // 0.0605 * 2^LUX_SCALE
        private const int TSL2561_LUX_K5C = (0x014d);  // 0.65 * 2^RATIO_SCALE
        private const int TSL2561_LUX_B5C = (0x0177);  // 0.0229 * 2^LUX_SCALE
        private const int TSL2561_LUX_M5C = (0x01dd);  // 0.0291 * 2^LUX_SCALE
        private const int TSL2561_LUX_K6C = (0x019a);  // 0.80 * 2^RATIO_SCALE
        private const int TSL2561_LUX_B6C = (0x0101);  // 0.0157 * 2^LUX_SCALE
        private const int TSL2561_LUX_M6C = (0x0127);  // 0.0180 * 2^LUX_SCALE
        private const int TSL2561_LUX_K7C = (0x029a);  // 1.3 * 2^RATIO_SCALE
        private const int TSL2561_LUX_B7C = (0x0037);  // 0.00338 * 2^LUX_SCALE
        private const int TSL2561_LUX_M7C = (0x002b);  // 0.00260 * 2^LUX_SCALE
        private const int TSL2561_LUX_K8C = (0x029a);  // 1.3 * 2^RATIO_SCALE
        private const int TSL2561_LUX_B8C = (0x0000);  // 0.000 * 2^LUX_SCALE
        private const int TSL2561_LUX_M8C = (0x0000);  // 0.000 * 2^LUX_SCALE

        // Auto-gain thresholds
        private const int TSL2561_AGC_THI_13MS = (4850);    // Max value at Ti 13ms = 5047
        private const int TSL2561_AGC_TLO_13MS = (100);
        private const int TSL2561_AGC_THI_101MS = (36000);   // Max value at Ti 101ms = 37177
        private const int TSL2561_AGC_TLO_101MS = (200);
        private const int TSL2561_AGC_THI_402MS = (63000);   // Max value at Ti 402ms = 65535
        private const int TSL2561_AGC_TLO_402MS = (500);

        // Clipping thresholds
        private const int TSL2561_CLIPPING_13MS = (4900);
        private const int TSL2561_CLIPPING_101MS = (37000);
        private const int TSL2561_CLIPPING_402MS = (65000);

        private const int TSL2561_REGISTER_CONTROL = 0x00;
        private const int TSL2561_REGISTER_TIMING = 0x01;
        private const int TSL2561_REGISTER_THRESHHOLDL_LOW = 0x02;
        private const int TSL2561_REGISTER_THRESHHOLDL_HIGH = 0x03;
        private const int TSL2561_REGISTER_THRESHHOLDH_LOW = 0x04;
        private const int TSL2561_REGISTER_THRESHHOLDH_HIGH = 0x05;
        private const int TSL2561_REGISTER_INTERRUPT = 0x06;
        private const int TSL2561_REGISTER_CRC = 0x08;
        private const int TSL2561_REGISTER_ID = 0x0A;
        private const int TSL2561_REGISTER_CHAN0_LOW = 0x0C;
        private const int TSL2561_REGISTER_CHAN0_HIGH = 0x0D;
        private const int TSL2561_REGISTER_CHAN1_LOW = 0x0E;
        private const int TSL2561_REGISTER_CHAN1_HIGH = 0x0F;

        private const int TSL2561_INTEGRATIONTIME_13MS = 0x00;    // 13.7ms
        private const int TSL2561_INTEGRATIONTIME_101MS = 0x01;    // 101ms
        private const int TSL2561_INTEGRATIONTIME_402MS = 0x02;    // 402ms

        private const int TSL2561_GAIN_1X = 0x00;    // No gain
        private const int TSL2561_GAIN_16X = 0x10;    // 16x gain


        private I2cDevice m_Device;

        /// <summary>
        /// Initializes the sensor
        /// </summary>
        /// <param name="device">The I2C device for communication with the sensor</param>
        public TSL2561(I2cDevice device)
        {
            m_Device = device;
        }

        public static int Address { get; } = TSL2561_ADDR_FLOAT;
        public static int Address_0 { get; } = TSL2561_ADDR_LOW;
        public static int Address_1 { get; } = TSL2561_ADDR_HIGH;
        public static byte FastTiming { get; } = TSL2561_INTEGRATIONTIME_13MS;
        public static byte MiddleTiming { get; } = TSL2561_INTEGRATIONTIME_101MS;
        public static byte SlowTiming { get; } = TSL2561_INTEGRATIONTIME_402MS;
        public static byte LowGain { get; } = TSL2561_GAIN_1X;
        public static byte HighGain { get; } = TSL2561_GAIN_16X;

        /// <summary>
        /// Reads data from data_0 and data_1 registers
        /// </summary>
        /// <returns>Contents of data_0 and data_1 registers</returns>
        public uint[] GetData()
        {
            var data = new uint[]
            {
                Read16(TSL2561_REGISTER_CHAN0_LOW),
                Read16(TSL2561_REGISTER_CHAN1_LOW)
            };
            return data;
        }

        /// <summary>
        /// Reads the sensor ID
        /// </summary>
        /// <returns>The sensor ID</returns>
        public byte GetId() => Read8(TSL2561_REGISTER_ID);

        /// <summary>
        /// Converts channel_0 and channel_1 data to LUX values using gain and ms
        /// </summary>
        /// <param name="gain">Specifies if high or low gain is used</param>
        /// <param name="timing">Specifies the timing used</param>
        /// <param name="ch0">Channel_0 data</param>
        /// <param name="ch1">Channel_1 data</param>
        /// <returns>LUX representation of channel_0 and channel_1 data</returns>
        public ulong GetLux(byte gain, uint timing, uint ch0, uint ch1)
        {
            /* Return 0 lux if the sensor is saturated */
            if ((ch0 > 0xFFFF) || (ch1 > 0xFFFF))
            {
                return 0;
            }

            /* Get the correct scale depending on the integration time */
            ulong scale = 0;
            switch (timing)
            {
                case TSL2561_INTEGRATIONTIME_13MS:
                    scale = TSL2561_LUX_CHSCALE_TINT0;
                    break;

                case TSL2561_INTEGRATIONTIME_101MS:
                    scale = TSL2561_LUX_CHSCALE_TINT1;
                    break;

                default:
                    scale = (1 << TSL2561_LUX_CHSCALE);
                    break;
            }

            // normalize to gain
            if (gain == TSL2561_GAIN_1X)
            {
                scale = scale << 4;
            }

            ulong d0 = (ch0 * scale) >> TSL2561_LUX_CHSCALE;
            ulong d1 = (ch1 * scale) >> TSL2561_LUX_CHSCALE;
            ulong ratio = 0;
            if (d0 != 0)
            {
                ratio = (d1 << (TSL2561_LUX_RATIOSCALE + 1)) / d0;
            }
            ratio = (ratio + 1) >> 1;

            // calculate LUX according to data-sheet
            var lux = 0UL;
            if (ratio <= TSL2561_LUX_K1C)
            {
                lux = TSL2561_LUX_B1C * d0 - d1 * TSL2561_LUX_M1C;
            }
            else if (ratio <= TSL2561_LUX_K2C)
            {
                lux = TSL2561_LUX_B2C * d0 - TSL2561_LUX_M2C * d1;
            }
            else if (ratio <= TSL2561_LUX_K3C)
            {
                lux = TSL2561_LUX_B3C * d0 - TSL2561_LUX_M3C * d1;
            }
            else if (ratio <= TSL2561_LUX_K4C)
            {
                lux = TSL2561_LUX_B4C * d0 - TSL2561_LUX_M4C * d1;
            }
            else if (ratio <= TSL2561_LUX_K5C)
            {
                lux = TSL2561_LUX_B5C * d0 - TSL2561_LUX_M5C * d1;
            }
            else if (ratio <= TSL2561_LUX_K6C)
            {
                lux = TSL2561_LUX_B6C * d0 - TSL2561_LUX_M6C * d1;
            }
            else if (ratio <= TSL2561_LUX_K7C)
            {
                lux = TSL2561_LUX_B7C * d0 - TSL2561_LUX_M7C * d1;
            }
            else if (ratio > TSL2561_LUX_K8C)
            {
                lux = TSL2561_LUX_B8C * d0 - TSL2561_LUX_M8C * d1;
            }
            lux = lux < 0 ? 0 : lux;
            lux += (1 << (TSL2561_LUX_LUXSCALE - 1));
            lux = lux >> TSL2561_LUX_LUXSCALE;

            return lux;
        }

        /// <summary>
        /// Power down the sensor
        /// </summary>
        public void PowerDown()
        {
            Write8(TSL2561_REGISTER_CONTROL, TSL2561_CONTROL_POWEROFF);
        }

        /// <summary>
        /// Power up the sensor
        /// </summary>
        public void PowerUp()
        {
            Write8(TSL2561_REGISTER_CONTROL, TSL2561_CONTROL_POWERON);
        }

        /// <summary>
        /// Set the timing to use for LUX measurement
        /// </summary>
        /// <param name="gain">Specifies if high or low gain shall be used</param>
        /// <param name="time">Specifies the timing: 0 => 14ms, 1 => 101ms, 2 => 402ms</param>
        /// <returns></returns>
        public void SetTiming(byte gain, byte time)
        {
            Write8(TSL2561_COMMAND_BIT | TSL2561_REGISTER_TIMING, (byte)(time | gain));
        }

        /// <summary>
        /// Reads 2 bytes from the specified address
        /// </summary>
        /// <param name="addr">Register to read from</param>
        /// <returns>2 byte register value</returns>
        private ushort Read16(byte addr)
        {
            var address = new byte[] { (byte)(addr | TSL2561_COMMAND_BIT) };
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
            var address = new byte[] { (byte)(addr | TSL2561_COMMAND_BIT) };
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
            var command = new byte[] { (byte)(addr | TSL2561_COMMAND_BIT), cmd };
            m_Device.Write(command);
        }
    }
}