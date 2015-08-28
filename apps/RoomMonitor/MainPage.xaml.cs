using System;
using System.ComponentModel;
using System.Text;
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
        private const string LUX_TOPIC = "/schuetz/lux";
        private const string TEMP_TOPIC = "/schuetz/temperature";

        private readonly MqttClient m_Client = new MqttClient("127.0.0.1", 1883, false, MqttSslProtocols.None);
        private readonly string m_ClientId = Guid.NewGuid().ToString();

        private int m_CurrentLuminosity;
        private int m_CurrentTemperature;

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;

            m_Client.Connect(m_ClientId);
            m_Client.ConnectionClosed += ClientConnectionClosed;
            m_Client.Subscribe(new[] { LUX_TOPIC, TEMP_TOPIC }, new byte[] { 1, 1 });
            m_Client.MqttMsgPublishReceived += ClientMessageReceived;
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

        private void ClientConnectionClosed(object sender, EventArgs e)
        {
        }

        private void ClientMessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var msg = Encoding.UTF8.GetString(e.Message);
            if (e.Topic.Equals(LUX_TOPIC))
            {
                int value;
                if (int.TryParse(msg, out value) && value != CurrentLuminosity)
                {
                    CurrentLuminosity = value;
                }
            }
            else if (e.Topic.Equals(TEMP_TOPIC))
            {
                int value;
                if (int.TryParse(msg, out value) && value != CurrentTemperature)
                {
                    CurrentTemperature = value;
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