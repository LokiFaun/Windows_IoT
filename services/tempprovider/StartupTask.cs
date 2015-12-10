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
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace tempprovider
{
    public sealed class StartupTask : IBackgroundTask, IDisposable
    {
        private const string m_DeviceSelector = "I2C1";
        private const string m_Host = "127.0.0.1";
        private const string m_TemperatureTopic = "/schuetz/temperature";
        private const string m_LocalWheaterService = "http://www.zamg.ac.at/ogd";
        private const string m_PressureTopic = "/schuetz/preassure";
        private const string m_AltitudeTopic = "/schuetz/altitude";
        private const string m_ServiceTopic = "/schuetz/services/preassure";
        private const string m_CsvColumnName = "\"LDred hPa\"";
        private const string m_CsvRowSearchString = "Eisenstadt";
        private const string m_ServiceName = "TempProviderService";
        private const byte m_QoS = 1;
        private const bool m_RetainMessage = true;
        private const int m_TimerDueTime = 1000;
        private const int m_TimerInterval = 2000;
        private const double m_MeanSeaLevelhPa = 1013.25;

        private readonly string m_ClientId = Guid.NewGuid().ToString();
        private readonly ManualResetEvent m_ShutdownEvent = new ManualResetEvent(false);
        private readonly Queue<double> m_TemperatureValues = new Queue<double>();
        private readonly Queue<double> m_PressureValues = new Queue<double>();
        private MqttClient m_Client = null;
        private BackgroundTaskDeferral m_Deferral = null;
        private AppServiceConnection m_AppServiceConnection;
        private bool m_IsRunning = true;
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
            await m_Sensor.PowerUp();

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

            var appService = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (appService != null && appService.Name == m_ServiceName)
            {
                m_AppServiceConnection = appService.AppServiceConnection;
                m_AppServiceConnection.RequestReceived += OnRequestReceived;
            }

            m_Client.Publish(m_ServiceTopic, Encoding.UTF8.GetBytes(DateTime.Now.ToUniversalTime().ToString("O")), m_QoS, true);

            // start timer
            m_Timer = new Timer(TemperatureProvider, null, m_TimerDueTime, m_TimerInterval);
        }

        private async void TemperatureProvider(object state)
        {
            // read the temperature values
            var currentTemperature = await m_Sensor.ReadTemperature();
            m_TemperatureValues.Enqueue(currentTemperature);
            while (m_TemperatureValues.Count > 10)
            {
                m_TemperatureValues.Dequeue();
            }
            var temperature = m_TemperatureValues.Average(x => x);

            // read the pressure values
            var currentPressure = await m_Sensor.ReadPressure();
            m_PressureValues.Enqueue(currentPressure);
            while (m_PressureValues.Count > 10)
            {
                m_PressureValues.Dequeue();
            }
            var pressure = m_PressureValues.Average(x => x);
            var seaLevelhPa = await RetrieveSeaLevelhPa();
            var altitude = m_Sensor.ReadAltitude(pressure, seaLevelhPa);

            Debug.WriteLine(string.Format("Publishing: temperature={0}, pressure={1}, altitude={2}", temperature, pressure / 100, altitude));
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
                m_Client.Publish(m_TemperatureTopic, Encoding.UTF8.GetBytes(temperature.ToString()), m_QoS, m_RetainMessage);
                m_Client.Publish(m_PressureTopic, Encoding.UTF8.GetBytes(pressure.ToString()), m_QoS, m_RetainMessage);
                m_Client.Publish(m_AltitudeTopic, Encoding.UTF8.GetBytes(altitude.ToString()), m_QoS, m_RetainMessage);
            }
            catch (MqttConnectionException ex)
            {
                // ignore exception
                Debug.WriteLine("Cannot connect to MQTT broker: " + ex.Message);
            }
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var message = args.Request.Message;
            string command = message["Command"] as string;
            switch (command)
            {
                case "GET":
                    OnGetRequestReceived(sender, args);
                    break;
                case "QUIT":
                    Close();
                    break;

            }
        }

        private async void OnGetRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageDeferral = args.GetDeferral();
            var returnMessage = new ValueSet();
            var pressure = m_PressureValues.Average(x => x);
            var temperature = m_TemperatureValues.Average(x => x);
            var seaLevelhPa = await RetrieveSeaLevelhPa();
            var altitude = m_Sensor.ReadAltitude(pressure, seaLevelhPa);
            returnMessage.Add("Temperature", temperature);
            returnMessage.Add("Pressure", pressure / 100);
            returnMessage.Add("Altitude", altitude);
            await args.Request.SendResponseAsync(returnMessage);
            messageDeferral.Complete();
        }

        private void Close()
        {
            if (m_IsRunning)
            {
                m_Deferral.Complete();
                m_IsRunning = false;
            }
        }

        private void TaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Close();
        }

        private async Task<double> RetrieveSeaLevelhPa()
        {
            var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync(m_LocalWheaterService);
                var csvFile = await response.Content.ReadAsStringAsync();
                var lines = csvFile.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 1)
                {
                    Debug.WriteLine("Cannot retrieve sea-level hPa: invalid CSV");
                    return m_MeanSeaLevelhPa;
                }
                var csvHeaders = lines[0].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var hPaIndex = csvHeaders.IndexOf(m_CsvColumnName);
                var line = lines.FirstOrDefault(x => x.Contains(m_CsvRowSearchString));
                var lineValues = line.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (hPaIndex > lineValues.Count)
                {
                    Debug.WriteLine("Cannot retrieve sea-level hPa: invalid CSV" );
                    return m_MeanSeaLevelhPa;
                }
                var value = lineValues[hPaIndex];
                return double.Parse(value);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("Cannot retrieve sea-level hPa: " + ex.Message);
                return m_MeanSeaLevelhPa;
            }
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
