using uPLibrary.Networking.M2Mqtt;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;

namespace mqttbroker
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const string m_ServiceName = "MqttBrokerService";

        private readonly MqttBroker m_Broker = new MqttBroker();
        private BackgroundTaskDeferral m_Deferral;
        private AppServiceConnection m_AppServiceConnection;
        private bool m_IsRunning = true;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            m_Deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstanceCanceled;

            var appService = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (appService != null && appService.Name == m_ServiceName)
            {
                m_AppServiceConnection = appService.AppServiceConnection;
                m_AppServiceConnection.RequestReceived += OnRequestReceived;
            }

            m_Broker.Start();
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var message = args.Request.Message;
            string command = message["Command"] as string;
            switch (command)
            {
                case "QUIT":
                    Close();
                    break;

            }
        }

        private void Close()
        {
            if (!m_IsRunning) { return; }

            if (m_Broker != null)
            {
                m_Broker.Stop();
            }
            m_Deferral.Complete();
            m_IsRunning = false;
        }

        private void TaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Close();
        }
    }
}