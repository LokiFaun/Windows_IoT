namespace Dashboard.Logic.Speech
{
    using News;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Telemetry;
    using View;
    using ViewModel;
    using Windows.ApplicationModel;
    using Windows.Globalization;
    using Windows.Media.Capture;
    using Windows.Media.SpeechRecognition;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// Handles speech recognition
    /// </summary>
    internal class SpeechInterpreter : IDisposable
    {
        /// <summary>
        /// The grammar file containing the commands
        /// </summary>
        private const string GrammerFile = "Grammar\\grammar.xml";

        /// <summary>
        /// The IoC container
        /// </summary>
        private readonly Container m_Container;

        /// <summary>
        /// The speech recognizer
        /// </summary>
        private readonly SpeechRecognizer m_Recognizer;

        /// <summary>
        /// Indicates whether this instance is disposed or not
        /// </summary>
        private bool m_IsDisposed = false;

        /// <summary>
        /// Initializes a new instance of <see cref="SpeechInterpreter"/>
        /// </summary>
        /// <param name="container">The IoC container</param>
        public SpeechInterpreter(Container container)
        {
            m_Container = container;

            m_Recognizer = new SpeechRecognizer(/*new Language("en-US")*/);
            m_Recognizer.ContinuousRecognitionSession.ResultGenerated += RecognizerResultGenerated;
            m_Recognizer.StateChanged += RecognizerStateChanged;
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the speech recognition
        /// </summary>
        public async void Start()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DateTimeProvider));
            }

            var hasPermission = await HasMicrophonePermission();
            if (!hasPermission)
            {
                throw new UnauthorizedAccessException("No access to microphone!");
            }

            var grammarFile = await Package.Current.InstalledLocation.GetFileAsync(GrammerFile);
            var grammarConstraint = new SpeechRecognitionGrammarFileConstraint(grammarFile);
            m_Recognizer.Constraints.Add(grammarConstraint);

            var compilationResult = await m_Recognizer.CompileConstraintsAsync();
            if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                await m_Recognizer.ContinuousRecognitionSession.StartAsync();
            }
        }

        /// <summary>
        /// Stops speech recognition
        /// </summary>
        public async void Stop()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DateTimeProvider));
            }

            await m_Recognizer.ContinuousRecognitionSession.StopAsync();
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
                m_Recognizer.Dispose();
            }
            m_IsDisposed = true;
        }

        /// <summary>
        /// Checks if the current user has the permission to use the microphone
        /// </summary>
        /// <returns><c>true</c> if permission is granted, else <c>false</c></returns>
        private async Task<bool> HasMicrophonePermission()
        {
            try
            {
                var settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio,
                    MediaCategory = MediaCategory.Speech
                };
                var capture = new MediaCapture();
                await capture.InitializeAsync(settings);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses the navigation page from the given string
        /// </summary>
        /// <param name="page">The string to parse</param>
        /// <returns>The navigation page or <c>null</c> if none is available</returns>
        private Type ParseNavigationPage(string page)
        {
            switch (page)
            {
                case MainViewModel.Name:
                    return typeof(MainPage);

                case NewsViewModel.Name:
                    return typeof(NewsPage);

                case SensorsViewModel.Name:
                    return typeof(SensorPage);

                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Handles successful recognized speech commands
        /// </summary>
        /// <param name="sender">The session of <see cref="SpeechContinuousRecognitionSession"/> that generated the result</param>
        /// <param name="args">The generated result arguments</param>
        private void RecognizerResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            var rssProvider = m_Container.Resolve<RssProvider>();

            var command = args.Result.SemanticInterpretation.Properties.ContainsKey("command") ?
                           args.Result.SemanticInterpretation.Properties["command"][0].ToString() :
                           "";
            var subreddit = args.Result.SemanticInterpretation.Properties.ContainsKey("subreddit") ?
                             args.Result.SemanticInterpretation.Properties["subreddit"][0].ToString() :
                             "";
            var page = args.Result.SemanticInterpretation.Properties.ContainsKey("page") ?
                             args.Result.SemanticInterpretation.Properties["page"][0].ToString() :
                             "";

            Debug.WriteLine(string.Format("Command: {0}, SubReddit: {1}, Page: {2}", command, subreddit, page));

            if (!string.IsNullOrWhiteSpace(subreddit) && (rssProvider != null))
            {
                rssProvider.Subreddit = subreddit;
            }


            if (!string.IsNullOrWhiteSpace(command))
            {
                switch (command)
                {
                    case "on":
                        DispatcherHelper.RunOnUIThread(() =>
                        {
                            var frame = m_Container.Resolve<Window>().Content as Frame;
                            frame.Navigate(typeof(MainPage));
                        });
                        break;

                    case "off":
                        DispatcherHelper.RunOnUIThread(() =>
                        {
                            var frame = m_Container.Resolve<Window>().Content as Frame;
                            frame.Navigate(typeof(BlankPage));
                        });
                        break;

                    default:
                        break;
                }
            }

            var navigationPage = ParseNavigationPage(page);
            if ((navigationPage != null))
            {
                DispatcherHelper.RunOnUIThread(() =>
                {
                    var frame = m_Container.Resolve<Window>().Content as Frame;
                    frame.Navigate(navigationPage);
                });
            }
        }

        /// <summary>
        /// Invoked when speech recognition state changes
        /// </summary>
        /// <param name="sender">The speech recognizer</param>
        /// <param name="args">The status arguments</param>
        private void RecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine(args.State);
        }
    }
}