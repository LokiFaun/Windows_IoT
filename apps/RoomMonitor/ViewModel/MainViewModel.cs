namespace Dashboard.ViewModel
{
    using Logic;
    using Logic.Weather;
    using System;
    using Windows.UI.Xaml.Media.Imaging;

    internal class MainViewModel : ViewModel
    {
        /// <summary>
        /// The name of the <see cref="MainViewModel"/>.
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
        /// The forecast city
        /// </summary>
        private string m_ForecastCity;

        /// <summary>
        /// The forecast icon
        /// </summary>
        private BitmapImage m_ForecastIcon;

        /// <summary>
        /// The forecast temperature
        /// </summary>
        private int m_ForecastTemperature;

        /// <summary>
        /// The weather forecast
        /// </summary>
        private string m_ForecastWeather;

        /// <summary>
        /// Initializes an instance of <see cref="MainViewModel"/>
        /// </summary>
        /// <param name="container"></param>
        public MainViewModel(Container container)
        {
            m_Container = container;

            CurrentDateTime = DateTime.Now;
            CurrentTemperature = 0;
            CurrentPressure = 0;
            CurrentLuminance = 0;

            ForecastIcon = new BitmapImage(new Uri(WeatherProvider.DefaultForecastIcon));
            ForecastTemperature = 0;
            ForecastWeather = "not available";
            ForecastCity = "not available";
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
        /// Gets or sets the forecast city
        /// </summary>
        public string ForecastCity
        {
            get { return m_ForecastCity; }
            set
            {
                m_ForecastCity = value;
                NotifyPropertyChanged(nameof(ForecastCity));
            }
        }

        /// <summary>
        /// Gets or sets the forecast icon
        /// </summary>
        public BitmapImage ForecastIcon
        {
            get { return m_ForecastIcon; }
            set
            {
                m_ForecastIcon = value;
                NotifyPropertyChanged(nameof(ForecastIcon));
            }
        }

        /// <summary>
        /// Gets or sets the forecast temperature
        /// </summary>
        public int ForecastTemperature
        {
            get { return m_ForecastTemperature; }
            set
            {
                m_ForecastTemperature = value;
                NotifyPropertyChanged(nameof(ForecastTemperature));
            }
        }

        /// <summary>
        /// Gets or sets the weather forecast
        /// </summary>
        public string ForecastWeather
        {
            get { return m_ForecastWeather; }
            set
            {
                m_ForecastWeather = value;
                NotifyPropertyChanged(nameof(ForecastWeather));
            }
        }
    }
}