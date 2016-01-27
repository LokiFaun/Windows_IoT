using Dashboard.ViewModel;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Syndication;

namespace Dashboard.Logic
{
    internal class RssProvider : IDisposable
    {
        private readonly Timer m_Timer;
        private bool m_IsDisposed = false;

        private const int Period = 15;

        private const int Infinite = -1;

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private async void Callback(object state)
        {
            var container = state as Container;
            if (container == null)
            {
                return;
            }

            var feed = new SyndicationFeed();
            var client = new HttpClient();
            var rssString = await client.GetStringAsync("http://www.reddit.com/r/worldnews/.rss");
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(rssString);
            feed.LoadFromXml(xmlDocument);

            var viewModel = container.ResolveNamed<MainViewModel>(MainViewModel.Name);
            if (viewModel == null)
            {
                return;
            }

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
        }
    }
}