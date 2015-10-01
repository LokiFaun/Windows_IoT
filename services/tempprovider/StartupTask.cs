using Windows.ApplicationModel.Background;
using Windows.Devices.I2c;
using System.Threading;
using System;
using System.Collections.Generic;
using uPLibrary.Networking.M2Mqtt;
using Windows.Devices.Enumeration;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using System.Diagnostics;
using System.Linq;
using System.Text;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace tempprovider
{
    public sealed class StartupTask : IBackgroundTask, IDisposable
    {
        private const string m_DeviceSelector = "I2C1";
        private const string m_Host = "127.0.0.1";
        private const string m_TemperatureTopic = "/schuetz/temperature";
        private const string m_PressureTopic = "/schuetz/preassure";
        private const byte m_QoS = 1;
        private const bool m_RetainMessage = true;
        private const int m_TimerDueTime = 1000;
        private const int m_TimerInterval = 2000;

        private readonly string m_ClientId = Guid.NewGuid().ToString();
        private readonly ManualResetEvent m_ShutdownEvent = new ManualResetEvent(false);
        private readonly Queue<double> m_TemperatureValues = new Queue<double>();
        private readonly Queue<double> m_PressureValues = new Queue<double>();
        private MqttClient m_Client = null;
        private BackgroundTaskDeferral m_Deferral = null;
        private I2cDevice m_Device = null;
        private Timer m_Timer = null;
        private BMP280 m_Sensor = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            m_Deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstanceCanceled;

            // initialize I2C
            var i2cSettings = new I2cConnectionSettings(BMP280.Address)
            {
                BusSpeed = I2cBusSpeed.FastMode,
                SharingMode = I2cSharingMode.Shared,
            };
            var selector = I2cDevice.GetDeviceSelector(m_DeviceSelector);
            var deviceInformation = await DeviceInformation.FindAllAsync(selector);
            m_Device = await I2cDevice.FromIdAsync(deviceInformation[0].Id, i2cSettings);

            // initialize sensor
            m_Sensor = new BMP280(m_Device);
            m_Sensor.PowerUp();

            // initialize MQTT
            try
            {
                m_Client = new MqttClient(m_Host);
                m_Client.Connect(m_ClientId);
            }
            catch (MqttConnectionException ex)
            {
                // ignore connection exception and retry to connect later
                Debug.WriteLine("Cannot connect to MQTT broker: " + ex.Message);
                m_Client = null;
            }

            // start timer
            m_Timer = new Timer(TemperatureProvider, null, m_TimerDueTime, m_TimerInterval);
        }

        private void TemperatureProvider(object state)
        {
            // read the temperature values
            var currentTemperature = m_Sensor.ReadTemperature();
            m_TemperatureValues.Enqueue(currentTemperature);
            while (m_TemperatureValues.Count > 10)
            {
                m_TemperatureValues.Dequeue();
            }
            var temperature = m_TemperatureValues.Average(x => x);

            // read the pressure values
            var currentPressure = m_Sensor.ReadPressure();
            m_PressureValues.Enqueue(currentPressure);
            while (m_PressureValues.Count > 10)
            {
                m_PressureValues.Dequeue();
            }
            var pressure = m_PressureValues.Average(x => x);

            // publish the sensor values
            try
            {
                if (m_Client == null)
                {
                    m_Client = new MqttClient(m_Host);
                }
                if (!m_Client.IsConnected)
                {
                    m_Client.Connect(m_ClientId);
                }
                Debug.WriteLine(string.Format("Publishing: temperature={0}, pressure={1}", temperature, pressure));
                m_Client.Publish(m_TemperatureTopic, Encoding.UTF8.GetBytes(temperature.ToString()), m_QoS, m_RetainMessage);
                m_Client.Publish(m_PressureTopic, Encoding.UTF8.GetBytes(pressure.ToString()), m_QoS, m_RetainMessage);
            }
            catch (MqttConnectionException ex)
            {
                // ignore exception
                Debug.WriteLine("Cannot connect to MQTT broker: " + ex.Message);
            }
        }

        private void TaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            m_Deferral.Complete();
        }

        #region IDisposable Support

        private bool disposedValue = false;

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (m_Timer != null)
                    {
                        m_Timer.Dispose();
                    }
                    if (m_Device != null)
                    {
                        m_Device.Dispose();
                    }
                    m_ShutdownEvent.Dispose();
                }

                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
