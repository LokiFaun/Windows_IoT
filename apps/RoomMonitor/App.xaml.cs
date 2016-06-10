namespace Dashboard
{
    using Logic.IO;
    using Logic.News;
    using Logic.Speech;
    using Logic.Telemetry;
    using Logic.Weather;
    using System;
    using uPLibrary.Networking.M2Mqtt;
    using View;
    using ViewModel;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private const string LocaterResource = "Locator";

        private const string MqttBrokerAddress = "schuetz-pi2";

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat application initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            // add needed objects to IoC container
            var locator = Resources[LocaterResource] as ViewModelLocator;
            if (locator != null)
            {
                var container = locator.Container;
                container.Register(locator);

                var dateTimeTask = new DateTimeProvider(container);
                container.Register(dateTimeTask);
                dateTimeTask.Start();

                var client = new MqttClient(MqttBrokerAddress);
                container.Register(client);

                var telemetryProvider = new TelemetryProvider(container);
                container.Register(telemetryProvider);

                var rssProvider = new RssProvider(container);
                container.Register(rssProvider);
                rssProvider.Start();

                var speechInterpreter = new SpeechInterpreter(container);
                container.Register(speechInterpreter);
                speechInterpreter.Start();

                var WeatherProvider = new WeatherProvider(container);
                container.Register(WeatherProvider);

                var ioProvider = new IOProvider(container);
                container.Register(ioProvider);
            }

            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
            if (locator != null)
            {
                var container = locator.Container;
                container.Register(Window.Current);
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity

            // clear all resources
            var locator = Resources[LocaterResource] as ViewModelLocator;
            if (locator != null)
            {
                var container = locator.Container;
                var dateTimeTask = container.Resolve<DateTimeProvider>();
                if (dateTimeTask != null)
                {
                    dateTimeTask.Dispose();
                }

                var telemetryProvider = container.Resolve<TelemetryProvider>();
                if (telemetryProvider != null)
                {
                    telemetryProvider.Dispose();
                }

                var rssProvider = container.Resolve<RssProvider>();
                if (rssProvider != null)
                {
                    rssProvider.Dispose();
                }

                var speecInterpreter = container.Resolve<SpeechInterpreter>();
                if (speecInterpreter != null)
                {
                    speecInterpreter.Dispose();
                }

                var weatherProvider = container.Resolve<WeatherProvider>();
                if (weatherProvider != null)
                {
                    weatherProvider.Dispose();
                }

                var ioProvider = container.Resolve<IOProvider>();
                if (ioProvider != null)
                {
                    ioProvider.Dispose();
                }
            }

            deferral.Complete();
        }
    }
}