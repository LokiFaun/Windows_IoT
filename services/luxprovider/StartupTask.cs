using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Foundation.Collections;

namespace luxprovider
{
    public sealed class StartupTask : IBackgroundTask, IDisposable
    {
        private const string m_DeviceSelector = "I2C1";
        private const string m_Host = "127.0.0.1";
        private const string m_LuxChannel1Topic = "/schuetz/lux/channel/1";
        private const string m_LuxChannel2Topic = "/schuetz/lux/channel/2";
        private const string m_LuxRatioTopic = "/schuetz/lux/ratio";
        private const string m_LuxTopic = "/schuetz/lux";
        private const string m_ServiceTopic = "/schuetz/services/lux";
        private const string m_ServiceName = "LuxProviderService";
        private const byte m_QoS = 1;
        private const bool m_RetainMessage = true;
        private const int m_TimerDueTime = 1000;
        private const int m_TimerInterval = 2000;

        private readonly string m_ClientId = Guid.NewGuid().ToString();
        private readonly byte m_Gain = TSL2561.HighGain;
        private readonly ManualResetEvent m_ShutdownEvent = new ManualResetEvent(false);
        private readonly byte m_Timing = TSL2561.FastTiming;
        private readonly Queue<ulong> m_LuxValues = new Queue<ulong>();
        private MqttClient m_Client = null;
        private BackgroundTaskDeferral m_Deferral = null;
        private AppServiceConnection m_AppServiceConnection;
        private bool m_IsRunning = true;
        private I2cDevice m_Device = null;
        private TSL2561 m_Sensor = null;
        private Timer m_Timer = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            m_Deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstanceCanceled;

            // initialize I2C
            var i2cSettings = new I2cConnectionSettings(TSL2561.Address)
            {
                BusSpeed = I2cBusSpeed.FastMode,
                SharingMode = I2cSharingMode.Shared,
            };
            var selector = I2cDevice.GetDeviceSelector(m_DeviceSelector);
            var deviceInformation = await DeviceInformation.FindAllAsync(selector);
            m_Device = await I2cDevice.FromIdAsync(deviceInformation[0].Id, i2cSettings);

            // initialize sensor
            m_Sensor = new TSL2561(m_Device);
            m_Sensor.SetTiming(m_Gain, m_Timing);
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

            var appService = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (appService != null && appService.Name == m_ServiceName)
            {
                m_AppServiceConnection = appService.AppServiceConnection;
                m_AppServiceConnection.RequestReceived += OnRequestReceived;
            }

            m_Client.Publish(m_ServiceTopic, Encoding.UTF8.GetBytes(DateTime.Now.ToUniversalTime().ToString("O")), m_QoS, true);

            // start timer
            m_Timer = new Timer(LuxProvider, null, m_TimerDueTime, m_TimerInterval);
        }

        /// <summary>
        /// Timer callback for periodically reading and publishing sensor values
        /// </summary>
        /// <param name="stateInfo">unused</param>
        private void LuxProvider(object stateInfo)
        {
            // read the sensor values
            var data = m_Sensor.GetData();
            var currentReading = m_Sensor.GetLux(m_Gain, m_Timing, data[0], data[1]);

            m_LuxValues.Enqueue(currentReading);
            while (m_LuxValues.Count > 10)
            {
                m_LuxValues.Dequeue();
            }
            var lux = (ulong)m_LuxValues.Average(x => (decimal)x);

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
                Debug.WriteLine(string.Format("Publishing: lux={0}, channel_0={1}, channel_1={2}, ratio={3}", lux, data[0], data[1], (data[1] / (double)data[0])));
                m_Client.Publish(m_LuxChannel1Topic, Encoding.UTF8.GetBytes(data[0].ToString()), m_QoS, m_RetainMessage);
                m_Client.Publish(m_LuxChannel2Topic, Encoding.UTF8.GetBytes(data[1].ToString()), m_QoS, m_RetainMessage);
                m_Client.Publish(m_LuxRatioTopic, Encoding.UTF8.GetBytes((data[1] / (double)data[0]).ToString()), m_QoS, m_RetainMessage);
                m_Client.Publish(m_LuxTopic, Encoding.UTF8.GetBytes(lux.ToString()), m_QoS, m_RetainMessage);
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
            var lux = (ulong)m_LuxValues.Average(x => (decimal)x);
            returnMessage.Add("Lux", lux);
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
                    if (m_Sensor != null)
                    {
                        m_Sensor.PowerDown();
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