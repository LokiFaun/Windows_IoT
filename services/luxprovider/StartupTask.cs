using System;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using Windows.ApplicationModel.Background;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace luxprovider
{
    public sealed class StartupTask : IBackgroundTask
    {
        private readonly ManualResetEvent m_ShutdownEvent = new ManualResetEvent(false);
        private Timer m_Timer = null;
        private BackgroundTaskDeferral m_Deferral = null;
        private I2cDevice m_Device = null;
        private TSL2561 m_Sensor = null;
        private MqttClient m_Client = null;
        private int m_MiliSeconds = 0;
        private bool m_Gain = false;
        private double m_CurrentLux = 0;

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
                var selector = I2cDevice.GetDeviceSelector("I2C1");
                var deviceInformation = await DeviceInformation.FindAllAsync(selector);
                m_Device = await I2cDevice.FromIdAsync(deviceInformation[0].Id, i2cSettings);

                m_Sensor = new TSL2561(m_Device);
                m_MiliSeconds = m_Sensor.SetTiming(m_Gain, 2);
                m_Sensor.PowerUp();

                m_Client = new MqttClient("test.mosquitto.org");
                m_Client.Connect(Guid.NewGuid().ToString());

                m_Timer = new Timer(LuxProvider, null, 1000, 5000);
            }
            catch (Exception)
            {
                taskInstance.Canceled -= TaskInstanceCanceled;
                CleanUp();
            }
        }

        private void LuxProvider(object stateInfo)
        {
            var data = m_Sensor.GetData();
            var lux = m_Sensor.GetLux(m_Gain, (uint)m_MiliSeconds, data[0], data[1]);
            //if (lux != m_CurrentLux)
            {
                m_CurrentLux = lux;
                m_Client.Publish("/schuetz/lux", Encoding.UTF8.GetBytes(lux.ToString()));
            }
        }

        private void CleanUp()
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
            m_Deferral.Complete();
        }

        private void TaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            CleanUp();
        }
    }
}