namespace Dashboard.Logic.Telemetry
{
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using ViewModel;

    /// <summary>
    /// Provides the view-model with telemetry data
    /// </summary>
    internal class TelemetryProvider : IDisposable
    {
        /// <summary>
        /// The MQTT topic the altitude is recieved on
        /// </summary>
        private const string m_AltitudeTopic = "/schuetz/altitude";

        /// <summary>
        /// The MQTT topic the luminance is recieved on
        /// </summary>
        private const string m_LuxTopic = "/schuetz/lux";

        /// <summary>
        /// The MQTT topic the pressure is recieved on
        /// </summary>
        private const string m_PressureTopic = "/schuetz/preassure";

        /// <summary>
        /// The MQTT topic the temperature is recieved on
        /// </summary>
        private const string m_TemperatureTopic = "/schuetz/temperature";

        /// <summary>
        /// The MQTT client
        /// </summary>
        private readonly MqttClient m_Client;

        /// <summary>
        /// The MQTT client ID
        /// </summary>
        private readonly string m_ClientId = Guid.NewGuid().ToString();

        /// <summary>
        /// The IoC container
        /// </summary>
        private readonly Container m_Container;

        /// <summary>
        /// The critical section
        /// </summary>
        private readonly object m_Lock = new object();

        /// <summary>
        /// The timer to add the current temperature and pressure values to the database
        /// </summary>
        private readonly Timer m_StorageTimer;

        /// <summary>
        /// The QoS for each topic
        /// </summary>
        private readonly IEnumerable<byte> QoS = new byte[]
        {
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
            MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE
        };

        /// <summary>
        /// All topics to subscribe to
        /// </summary>
        private readonly IEnumerable<string> Topics = new[]
        {
            m_LuxTopic,
            m_TemperatureTopic,
            m_AltitudeTopic,
            m_PressureTopic,
        };

        /// <summary>
        /// The current pressure
        /// </summary>
        private double m_CurrentPressure;

        /// <summary>
        /// The current temperature
        /// </summary>
        private double m_CurrentTemperature;

        /// <summary>
        /// Indicates whether the instance is disposed or not
        /// </summary>
        private bool m_IsDisposed = false;

        /// <summary>
        /// Initializes a new instance of <see cref="TelemetryProvider"/>
        /// </summary>
        /// <param name="container">The IoC container</param>
        public TelemetryProvider(Container container)
        {
            m_Container = container;

            var dueTime = TimeSpan.FromMinutes(60 - DateTime.Now.Minute);
            m_StorageTimer = new Timer(HandleStorageTimer, null, dueTime, TimeSpan.FromHours(2));

            m_Client = container.Resolve<MqttClient>();
            Task.Factory.StartNew(EstablishConnection);
        }

        /// <summary>
        /// Disposes the instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the instance references
        /// </summary>
        /// <param name="disposing"><c>true</c> to dispose references</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                m_Client.Disconnect();
                m_StorageTimer.Dispose();
            }
            m_IsDisposed = true;
        }

        /// <summary>
        /// Establishes a connection to the MQTT broker
        /// </summary>
        private void EstablishConnection()
        {
            m_Client.Connect(m_ClientId);
            m_Client.Subscribe(Topics.ToArray(), QoS.ToArray());
            m_Client.MqttMsgPublishReceived += MessageReceived;
        }

        /// <summary>
        /// Handles a new message on the altitude topic
        /// </summary>
        /// <param name="message">The received message</param>
        private void HandleAltitudeMessage(string message)
        {
            double value;
            if (double.TryParse(message, out value))
            {
                DispatcherHelper.RunOnUIThread(() =>
                {
                    var viewModel = m_Container.ResolveNamed<MainViewModel>(MainViewModel.Name);
                    if (viewModel != null)
                    {
                        viewModel.CurrentAltitude = value;
                    }
                });
            }
        }

        /// <summary>
        /// Handles a new message on the luminance topic
        /// </summary>
        /// <param name="message">The received message</param>
        private void HandleLuxMessage(string message)
        {
            ulong value;
            if (ulong.TryParse(message, out value))
            {
                DispatcherHelper.RunOnUIThread(() =>
                {
                    var viewModel = m_Container.ResolveNamed<MainViewModel>(MainViewModel.Name);
                    if (viewModel != null)
                    {
                        viewModel.CurrentLuminance = value;
                    }
                });
            }
        }

        /// <summary>
        /// Handles a new message on the pressure topic
        /// </summary>
        /// <param name="message">The received message</param>
        private void HandlePressureMessage(string message)
        {
            double value;
            if (double.TryParse(message, out value))
            {
                lock (m_Lock)
                {
                    m_CurrentPressure = value;
                }

                DispatcherHelper.RunOnUIThread(() =>
                {
                    var viewModel = m_Container.ResolveNamed<MainViewModel>(MainViewModel.Name);
                    if (viewModel != null)
                    {
                        viewModel.CurrentPressure = value;
                    }
                });
            }
        }

        /// <summary>
        /// Handles the <see cref="m_StorageTimer"/> timeout callback
        /// </summary>
        /// <param name="state">not used</param>
        private void HandleStorageTimer(object state)
        {
            lock (m_Lock)
            {
                using (var repository = new TelemetryStorage(m_Container))
                {
                    repository.AddTemperature(new Temperature(m_CurrentTemperature));
                    repository.AddPressure(new Pressure(m_CurrentPressure));
                }
            }
        }

        /// <summary>
        /// Handles a new message on the temperature topic
        /// </summary>
        /// <param name="message">The received message</param>
        private void HandleTemperatureMessage(string message)
        {
            double value;
            if (double.TryParse(message, out value))
            {
                lock (m_Lock)
                {
                    m_CurrentTemperature = value;
                }

                DispatcherHelper.RunOnUIThread(() =>
                {
                    var viewModel = m_Container.ResolveNamed<MainViewModel>(MainViewModel.Name);
                    if (viewModel != null)
                    {
                        viewModel.CurrentTemperature = value;
                    }
                });
            }
        }

        /// <summary>
        /// Invoked when a new MQTT message is received
        /// </summary>
        /// <param name="sender">The sending <see cref="MqttClient"/></param>
        /// <param name="e">The event args</param>
        private void MessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message);
            switch (e.Topic)
            {
                case m_LuxTopic:
                    HandleLuxMessage(message);
                    break;

                case m_PressureTopic:
                    HandlePressureMessage(message);
                    break;

                case m_TemperatureTopic:
                    HandleTemperatureMessage(message);
                    break;

                case m_AltitudeTopic:
                    HandleAltitudeMessage(message);
                    break;
            }
        }
    }
}