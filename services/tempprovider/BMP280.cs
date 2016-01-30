using System;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace tempprovider
{
    internal sealed class BMP280
    {
        private const int REGISTER_DIG_T1 = 0x88;
        private const int REGISTER_DIG_T2 = 0x8A;
        private const int REGISTER_DIG_T3 = 0x8C;

        private const int REGISTER_DIG_P1 = 0x8E;
        private const int REGISTER_DIG_P2 = 0x90;
        private const int REGISTER_DIG_P3 = 0x92;
        private const int REGISTER_DIG_P4 = 0x94;
        private const int REGISTER_DIG_P5 = 0x96;
        private const int REGISTER_DIG_P6 = 0x98;
        private const int REGISTER_DIG_P7 = 0x9A;
        private const int REGISTER_DIG_P8 = 0x9C;
        private const int REGISTER_DIG_P9 = 0x9E;

        private const int REGISTER_CHIPID = 0xD0;
        private const int REGISTER_VERSION = 0xD1;
        private const int REGISTER_SOFTRESET = 0xE0;

        private const int REGISTER_CAL26 = 0xE1;

        private const int REGISTER_CONTROL_HUMIDITY = 0xF2;
        private const int REGISTER_CONTROL = 0xF4;
        private const int REGISTER_CONFIG = 0xF5;
        private const int REGISTER_PRESSUREDATA_MSB = 0xF7;
        private const int REGISTER_PRESSUREDATA_LSB = 0xF8;
        private const int REGISTER_PRESSUREDATA_XLSB = 0xF9;
        private const int REGISTER_TEMPDATA_MSB = 0xFA;
        private const int REGISTER_TEMPDATA_LSB = 0xFB;
        private const int REGISTER_TEMPDATA_XLSB = 0xFC;
        private const int REGISTER_HUMIDITYDATA_MSB = 0xFD;
        private const int REGISTER_HUMIDITYDATA_LSB = 0xFE;

        private struct CalibrationData
        {
            public ushort T1;
            public short T2;
            public short T3;

            public ushort P1;
            public short P2;
            public short P3;
            public short P4;
            public short P5;
            public short P6;
            public short P7;
            public short P8;
            public short P9;
        }

        private bool m_IsInitialized = false;
        private int m_Normalized = int.MinValue;
        private I2cDevice m_Device;
        private CalibrationData m_Calibration = new CalibrationData();

        public static int Address { get; } = 0x76;

        /// <summary>
        /// Initializes the sensor
        /// </summary>
        /// <param name="device">The I2C device for communication with the sensor</param>
        public BMP280(I2cDevice device)
        {
            m_Device = device;
        }

        /// <summary>
        /// Powers up the sensor
        /// </summary>
        /// <returns>false in case of error, otherwise false</returns>
        public async Task PowerUp()
        {
            if (Read8(REGISTER_CHIPID) != 0x58)
            {
                return;
            }
            await ReadCoefficients();
            Write8(REGISTER_CONTROL, 0x3F);
            await Task.Delay(1);
            Write8(REGISTER_CONTROL_HUMIDITY, 0x03);
            await Task.Delay(1);

            m_IsInitialized = true;
        }

        /// <summary>
        /// Read the temperature from the sensor
        /// </summary>
        /// <returns>The temperature in °C</returns>
        public async Task<double> ReadTemperature()
        {
            if (!m_IsInitialized)
            {
                await PowerUp();
            }
            var msb = Read8(REGISTER_TEMPDATA_MSB);
            var lsb = Read8(REGISTER_TEMPDATA_LSB);
            var xlsb = Read8(REGISTER_TEMPDATA_XLSB);

            int adcTemperature = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            var var1 = ((((adcTemperature >> 3) - (m_Calibration.T1 << 1))) *
                m_Calibration.T2) >> 11;
            var var2 = (((((adcTemperature >> 4) - m_Calibration.T1) *
                ((adcTemperature >> 4) - m_Calibration.T1)) >> 12) *
                m_Calibration.T3) >> 14;
            m_Normalized = var1 + var2;
            var temperature = (float)((m_Normalized * 5 + 128) >> 8);
            return temperature / 100;
        }

        public async Task<double> ReadPressure()
        {
            if (!m_IsInitialized)
            {
                await PowerUp();
            }

            var msb = Read8(REGISTER_PRESSUREDATA_MSB);
            var lsb = Read8(REGISTER_PRESSUREDATA_LSB);
            var xlsb = Read8(REGISTER_PRESSUREDATA_XLSB);
            int adcPressure = (msb << 12) + (lsb << 4) + (xlsb >> 4);

            var var1 = (long)m_Normalized - 128000;
            var var2 = var1 * var1 * m_Calibration.P6;
            var2 = var2 + (var1 * m_Calibration.P4 << 17);
            var2 = var2 + ((long)m_Calibration.P4 << 35);
            var1 = ((var1 * var1 * m_Calibration.P3) >> 8) +
                ((var1 * m_Calibration.P2) << 12);
            var1 = ((((long)1 << 47) + var1) * m_Calibration.P1) >> 33;

            if (var1 == 0)
            {
                return 0;
            }
            var pressure = (long)1048576 - adcPressure;
            pressure = (((pressure << 31) - var2) * 3125) / var1;
            var1 = (m_Calibration.P9 * (pressure >> 13) * (pressure >> 13)) >> 25;
            var2 = (m_Calibration.P8 * pressure) >> 19;
            pressure = ((pressure + var1 + var2) >> 8) + (m_Calibration.P7 << 4);

            pressure /= 256;
            return pressure / 100.0;
        }

        public double CalculateAltitude(double pressure, double seaLevelhPa) =>
            44330.0 * (1.0 - Math.Pow(pressure / seaLevelhPa, 0.1903));

        private async Task ReadCoefficients()
        {
            m_Calibration.T1 = Read16LittleEndian(REGISTER_DIG_T1);
            m_Calibration.T2 = (short)Read16LittleEndian(REGISTER_DIG_T2);
            m_Calibration.T3 = (short)Read16LittleEndian(REGISTER_DIG_T3);

            m_Calibration.P1 = Read16LittleEndian(REGISTER_DIG_P1);
            m_Calibration.P2 = (short)Read16LittleEndian(REGISTER_DIG_P2);
            m_Calibration.P3 = (short)Read16LittleEndian(REGISTER_DIG_P3);
            m_Calibration.P4 = (short)Read16LittleEndian(REGISTER_DIG_P4);
            m_Calibration.P5 = (short)Read16LittleEndian(REGISTER_DIG_P5);
            m_Calibration.P6 = (short)Read16LittleEndian(REGISTER_DIG_P6);
            m_Calibration.P7 = (short)Read16LittleEndian(REGISTER_DIG_P7);
            m_Calibration.P8 = (short)Read16LittleEndian(REGISTER_DIG_P8);
            m_Calibration.P9 = (short)Read16LittleEndian(REGISTER_DIG_P9);

            await Task.Delay(1);
        }

        /// <summary>
        /// Reads 2 bytes in little endian format from the specified address
        /// </summary>
        /// <param name="addr">Register to read from</param>
        /// <returns>2 byte register value</returns>
        private ushort Read16LittleEndian(byte addr)
        {
            var address = new byte[] { addr };
            var data = new byte[2];

            m_Device.WriteRead(address, data);

            int h = data[1] << 8;
            int l = data[0];
            return (ushort)(h + l);
        }

        /// <summary>
        /// Read 1 byte form the specified address
        /// </summary>
        /// <param name="addr">Register to read from</param>
        /// <returns>1 byte register value</returns>
        private byte Read8(byte addr)
        {
            var address = new byte[] { addr };
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
            var command = new byte[] { addr, cmd };
            m_Device.Write(command);
        }
    }
}