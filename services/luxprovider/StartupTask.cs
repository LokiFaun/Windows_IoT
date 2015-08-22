﻿using System;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using Windows.ApplicationModel.Background;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace luxprovider
{
    public sealed class StartupTask : IBackgroundTask, IDisposable
    {
        private const string m_DeviceSelector = "I2C1";
        private const bool m_Gain = false;
        private const string m_Host = "test.mosquitto.org";
        private const string m_LuxChannel1Topic = "/schuetz/lux/channel/1";
        private const string m_LuxChannel2Topic = "/schuetz/lux/channel/2";
        private const string m_LuxRatioTopic = "schuetz/lux/ratio";
        private const string m_LuxTopic = "schuetz/lux";
        private const byte m_QoS = 1;
        private const bool m_RetainMessage = true;
        private const int m_TimerDueTime = 1000;
        private const int m_TimerInterval = 2000;

        private readonly ManualResetEvent m_ShutdownEvent = new ManualResetEvent(false);
        private MqttClient m_Client = null;
        private BackgroundTaskDeferral m_Deferral = null;
        private I2cDevice m_Device = null;
        private int m_MiliSeconds = 0;
        private TSL2561 m_Sensor = null;
        private Timer m_Timer = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            m_Deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstanceCanceled;

            try
            {
                var i2cSettings = new I2cConnectionSettings(TSL2561.ADDRESS)
                {
                    BusSpeed = I2cBusSpeed.FastMode,
                    SharingMode = I2cSharingMode.Shared,
                };
                var selector = I2cDevice.GetDeviceSelector(m_DeviceSelector);
                var deviceInformation = await DeviceInformation.FindAllAsync(selector);
                m_Device = await I2cDevice.FromIdAsync(deviceInformation[0].Id, i2cSettings);

                m_Sensor = new TSL2561(m_Device);
                m_MiliSeconds = m_Sensor.SetTiming(m_Gain, TSL2561.SLOW_TIMING);
                m_Sensor.PowerUp();

                m_Client = new MqttClient(m_Host);
                m_Client.Connect(Guid.NewGuid().ToString());

                m_Timer = new Timer(LuxProvider, null, m_TimerDueTime, m_TimerInterval);
            }
            catch (Exception)
            {
                taskInstance.Canceled -= TaskInstanceCanceled;
                m_Deferral.Complete();
            }
        }

        private void LuxProvider(object stateInfo)
        {
            var data = m_Sensor.GetData();
            var lux = m_Sensor.GetLux(m_Gain, (uint)m_MiliSeconds, data[0], data[1]);

            m_Client.Publish(m_LuxChannel1Topic, Encoding.UTF8.GetBytes(data[0].ToString()), m_QoS, m_RetainMessage);
            m_Client.Publish(m_LuxChannel2Topic, Encoding.UTF8.GetBytes(data[1].ToString()), m_QoS, m_RetainMessage);
            m_Client.Publish(m_LuxRatioTopic, Encoding.UTF8.GetBytes((data[1] / (double)data[0]).ToString()), m_QoS, m_RetainMessage);
            m_Client.Publish(m_LuxTopic, Encoding.UTF8.GetBytes(lux.ToString()), m_QoS, m_RetainMessage);
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