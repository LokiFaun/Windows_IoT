namespace Dashboard.Logic
{
    using System;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;

    /// <summary>
    /// Helper class for executing an action on the UI thread
    /// </summary>
    internal static class DispatcherHelper
    {
        /// <summary>
        /// Executes the specified action on the UI thread
        /// </summary>
        /// <param name="agileCallback">The action to execute</param>
        public static async void RunOnUIThread(DispatchedHandler agileCallback)
        {
            var currentWindow = CoreApplication.MainView;
            if (currentWindow == null)
            {
                return;
            }

            var dispatcher = currentWindow.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                agileCallback.Invoke();
            });
        }
    }
}
