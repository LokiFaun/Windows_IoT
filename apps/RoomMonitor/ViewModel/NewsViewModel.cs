namespace Dashboard.ViewModel
{
    using Logic;
    using System.Collections.Generic;
    using Windows.Web.Syndication;

    /// <summary>
    /// Holds data for display on the 'news' page
    /// </summary>
    internal class NewsViewModel : ViewModel
    {
        /// <summary>
        /// The name of the MainViewModel.
        /// </summary>
        public const string Name = "News";

        /// <summary>
        /// The RSS feed items
        /// </summary>
        private IEnumerable<SyndicationItem> m_FeedItems;

        /// <summary>
        /// The IoC container
        /// </summary>
        private readonly Container m_Container;

        /// <summary>
        /// Initializes a new instance of <see cref="NewsViewModel"/>
        /// </summary>
        /// <param name="container">The IoC container</param>
        public NewsViewModel(Container container)
        {
            m_Container = container;
        }

        /// <summary>
        /// Gets or sets the RSS feed items
        /// </summary>
        public IEnumerable<SyndicationItem> FeedItems
        {
            get { return m_FeedItems; }
            set
            {
                m_FeedItems = value;
                NotifyPropertyChanged(nameof(FeedItems));
            }
        }
    }
}