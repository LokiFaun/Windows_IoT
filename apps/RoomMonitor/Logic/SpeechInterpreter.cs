namespace Dashboard.Logic
{
    using Dashboard.View;
    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
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
        /// The IoC container
        /// </summary>
        private readonly Container m_Container;

        /// <summary>
        /// The speech recognizer
        /// </summary>
        private readonly SpeechRecognizer m_Recognizer;

        /// <summary>
        /// Indicates wheter this instance is disposed or not
        /// </summary>
        private bool m_IsDisposed = false;

        /// <summary>
        /// Intializes a new instance of <see cref="SpeechInterpreter"/>
        /// </summary>
        /// <param name="container">The IoC container</param>
        public SpeechInterpreter(Container container)
        {
            m_Container = container;

            m_Recognizer = new SpeechRecognizer(/*new Language("en-US")*/);
            m_Recognizer.ContinuousRecognitionSession.ResultGenerated += RecognizerResultGenerated;
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

            var grammarFile = await Package.Current.InstalledLocation.GetFileAsync("Grammar\\grammar.xml");
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
        /// Handles successful recognized speech commands
        /// </summary>
        /// <param name="sender">The session of <see cref="SpeechContinuousRecognitionSession"/> that generated the result</param>
        /// <param name="args">The generated result arguments</param>
        private void RecognizerResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            var frame = Window.Current.Content as Frame;
            var rssProvider = m_Container.Resolve<RssProvider>();

            var subreddit = args.Result.SemanticInterpretation.Properties.ContainsKey("subreddit") ?
                             args.Result.SemanticInterpretation.Properties["subreddit"][0].ToString() :
                             "";
            var page = args.Result.SemanticInterpretation.Properties.ContainsKey("page") ?
                             args.Result.SemanticInterpretation.Properties["page"][0].ToString() :
                             "";

            if (!string.IsNullOrWhiteSpace(subreddit) && (rssProvider != null))
            {
                rssProvider.Subreddit = subreddit;
            }

            if (!string.IsNullOrWhiteSpace(page) && (frame != null))
            {
                switch (page)
                {
                    case "mainpage":
                        frame.Navigate(typeof(MainPage));
                        break;

                    case "sensorpage":
                        frame.Navigate(typeof(SensorPage));
                        break;

                    case "screensaver":
                        frame.Navigate(typeof(BlankPage));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}