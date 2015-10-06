using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RoomMonitor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private const string m_TopicLux = "/schuetz/lux";
        private const string m_TopicTemperature = "/schuetz/temperature";
        private const string m_TopicPressure = "/schuetz/pressure";

        private readonly MqttClient m_Client = new MqttClient("127.0.0.1", 1883, false, MqttSslProtocols.None);
        private readonly string m_ClientId = Guid.NewGuid().ToString();

        private int m_CurrentLuminosity;
        private int m_CurrentTemperature;
        private double m_CurrentPressure;

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;

            m_Client.Connect(m_ClientId);
            m_Client.ConnectionClosed += ClientConnectionClosed;
            m_Client.MqttMsgPublishReceived += ClientMessageReceived;
            m_Client.Subscribe(new[] { m_TopicLux, m_TopicTemperature, m_TopicPressure }, new byte[] { 1, 1 });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int CurrentLuminosity
        {
            get { return m_CurrentLuminosity; }
            set
            {
                m_CurrentLuminosity = value;
                NotifyOnPropertyChanged(nameof(CurrentLuminosity));
            }
        }

        public int CurrentTemperature
        {
            get { return m_CurrentTemperature; }
            set
            {
                m_CurrentTemperature = value;
                NotifyOnPropertyChanged(nameof(CurrentTemperature));
            }
        }

        public double CurrentPressure
        {
            get { return m_CurrentPressure; }
            set
            {
                m_CurrentPressure = value;
                NotifyOnPropertyChanged(nameof(CurrentPressure));
            }
        }

        private void ClientConnectionClosed(object sender, EventArgs e)
        {
            while (!m_Client.IsConnected)
            {
                m_Client.Connect(m_ClientId);
                m_Client.Subscribe(new[] { m_TopicLux, m_TopicTemperature, m_TopicPressure }, new byte[] { 1, 1 });
            }
        }

        private void ClientMessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var msg = Encoding.UTF8.GetString(e.Message);
            if (e.Topic.Equals(m_TopicLux))
            {
                int value;
                if (int.TryParse(msg, out value) && value != CurrentLuminosity)
                {
                    CurrentLuminosity = value;
                }
            }
            else if (e.Topic.Equals(m_TopicTemperature))
            {
                int value;
                if (int.TryParse(msg, out value) && value != CurrentTemperature)
                {
                    CurrentTemperature = value;
                }
            }
            else if (e.Topic.Equals(m_TopicPressure))
            {
                double value;
                if (double.TryParse(msg, out value) && value != CurrentPressure)
                {
                    CurrentPressure = value;
                }
            }
        }

        private async void NotifyOnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                if (Dispatcher.HasThreadAccess)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                     {
                         handler(this, new PropertyChangedEventArgs(propertyName));
                     });
                }
            }
        }
    }
}