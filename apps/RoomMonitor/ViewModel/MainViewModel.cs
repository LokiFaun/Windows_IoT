namespace Dashboard.ViewModel
{
    using Logic;
    using System;
    using System.Collections.Generic;
    using Windows.Web.Syndication;

    internal class MainViewModel : ViewModel
    {
        /// <summary>
        /// The name of the MainViewModel.
        /// </summary>
        public const string Name = "Main";

        /// <summary>
        /// The IoC container
        /// </summary>
        private readonly Container m_Container;

        /// <summary>
        /// The current altitude
        /// </summary>
        private double m_CurrentAltitude;

        /// <summary>
        /// The current date/time
        /// </summary>
        private DateTime m_CurrentDateTime;

        /// <summary>
        /// The current luminance
        /// </summary>
        private double m_CurrentLuminance;

        /// <summary>
        /// The current pressure
        /// </summary>
        private double m_CurrentPressure;

        /// <summary>
        /// The current temperature
        /// </summary>
        private double m_CurrentTemperature;

        /// <summary>
        /// The RSS feed items
        /// </summary>
        private IEnumerable<SyndicationItem> m_FeedItems;

        /// <summary>
        /// Initializes an instance of <see cref="MainViewModel"/>
        /// </summary>
        /// <param name="container"></param>
        public MainViewModel(Container container)
        {
            CurrentDateTime = DateTime.Now;

            m_Container = container;
            var locator = container.ResolveNamed<ViewModelLocator>("Locator");
            if (locator != null && locator.InDesignMode)
            {
                CurrentTemperature = 21.5;
                CurrentPressure = 1025.5;
                CurrentLuminance = 3000;
            }
            else
            {
                CurrentTemperature = 0;
                CurrentPressure = 0;
                CurrentLuminance = 0;
            }
        }

        /// <summary>
        /// Gets or sets the current altitude
        /// </summary>
        public double CurrentAltitude
        {
            get { return m_CurrentAltitude; }
            set
            {
                m_CurrentAltitude = value;
                NotifyPropertyChanged(nameof(CurrentAltitude));
            }
        }

        /// <summary>
        /// Gets or sets the current date/time
        /// </summary>
        public DateTime CurrentDateTime
        {
            get { return m_CurrentDateTime; }
            set
            {
                m_CurrentDateTime = value;
                NotifyPropertyChanged(nameof(CurrentDateTime));
            }
        }

        /// <summary>
        /// Gets or sets the current luminance
        /// </summary>
        public double CurrentLuminance
        {
            get { return m_CurrentLuminance; }
            set
            {
                m_CurrentLuminance = value;
                NotifyPropertyChanged(nameof(CurrentLuminance));
            }
        }

        /// <summary>
        /// Gets or sets the current pressure
        /// </summary>
        public double CurrentPressure
        {
            get { return m_CurrentPressure; }
            set
            {
                m_CurrentPressure = value;
                NotifyPropertyChanged(nameof(CurrentPressure));
            }
        }

        /// <summary>
        /// Gets or sets the current temperature
        /// </summary>
        public double CurrentTemperature
        {
            get { return m_CurrentTemperature; }
            set
            {
                m_CurrentTemperature = value;
                NotifyPropertyChanged(nameof(CurrentTemperature));
            }
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