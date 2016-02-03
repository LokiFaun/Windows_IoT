namespace Dashboard.Logic
{
    using Dashboard.ViewModel;
    using System;
    using System.Net.Http;
    using System.Threading;
    using Windows.Data.Xml.Dom;
    using Windows.Web.Syndication;

    /// <summary>
    /// Provider for displaying the newest submissions to 'r/worldnews/
    /// </summary>
    internal class RssProvider : IDisposable
    {
        /// <summary>
        /// The timer
        /// </summary>
        private readonly Timer m_Timer;

        /// <summary>
        /// Indicates wheter the object is disposed or not
        /// </summary>
        private bool m_IsDisposed = false;

        /// <summary>
        /// Specifies the update period in minutes
        /// </summary>
        private const int Period = 15;

        /// <summary>
        /// Specifies the infinite period for stopping the timer
        /// </summary>
        private const int Infinite = -1;

        /// <summary>
        /// Specifies the subreddit to retrieve the feed from
        /// </summary>
        private string m_Subreddit = "worldnews";

        /// <summary>
        /// Critical section
        /// </summary>
        private object m_Lock = new object();

        /// <summary>
        /// Gets or sets the Subreddit
        /// </summary>
        public string Subreddit
        {
            get
            {
                lock (m_Lock)
                {
                    return m_Subreddit;
                }
            }
            set
            {
                lock (m_Lock)
                {
                    m_Subreddit = value;
                    m_Timer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(Period));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RssProvider"/>
        /// </summary>
        /// <param name="container">The IoC container</param>
        public RssProvider(Container container)
        {
            m_Timer = new Timer(Callback, container, Infinite, Infinite);
        }

        /// <summary>
        /// Starts the update task
        /// </summary>
        public void Start()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DateTimeProvider));
            }

            m_Timer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(Period));
        }

        /// <summary>
        /// Stops the update task
        /// </summary>
        public void Stop()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DateTimeProvider));
            }

            m_Timer.Change(Infinite, Infinite);
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
        /// The timer callback for periodic updates
        /// </summary>
        /// <param name="state">A reference to the IoC <see cref="Container"/></param>
        private async void Callback(object state)
        {
            var container = state as Container;
            if (container == null)
            {
                return;
            }

            var viewModel = container.ResolveNamed<MainViewModel>(MainViewModel.Name);
            if (viewModel == null)
            {
                return;
            }

            var url = string.Empty;
            lock (m_Lock)
            {
                url = string.Format("http://www.reddit.com/r/{0}/new/.rss", m_Subreddit);
            }

            var feed = new SyndicationFeed();
            var client = new HttpClient();
            var rssString = await client.GetStringAsync(url);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(rssString);
            feed.LoadFromXml(xmlDocument);

            DispatcherHelper.RunOnUIThread(() =>
            {
                viewModel.FeedItems = feed.Items;
            });
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
                m_Timer.Dispose();
            }
            m_IsDisposed = true;
        }
    }
}