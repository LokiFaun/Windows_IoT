using uPLibrary.Networking.M2Mqtt;
using Windows.ApplicationModel.Background;

namespace mqttbroker
{
    public sealed class StartupTask : IBackgroundTask
    {
        private MqttBroker m_Broker = null;
        private BackgroundTaskDeferral m_Deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            m_Deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstanceCanceled;

            m_Broker = new MqttBroker();
            m_Broker.Start();
        }

        private void TaskInstanceCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (m_Broker != null)
            {
                m_Broker.Stop();
            }
            m_Deferral.Complete();
        }
    }
}