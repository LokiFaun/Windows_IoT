using Dashboard.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Windows.UI.Xaml.Media.Imaging;

namespace Dashboard.Logic.Weather
{
    internal class WeatherProvider : IDisposable
    {
        /// <summary>
        /// The default forecast icon
        /// </summary>
        public const string DefaultForecastIcon = "ms-appx:///Resources/Icon/na.png";

        /// <summary>
        /// The api key file
        /// </summary>
        private const string ApiKeyFile = "Resources/WeatherApiKey.txt";

        /// <summary>
        /// The base url for a forecast
        /// </summary>
        private const string ForecastUri = "http://dsx.weather.com/%28wxd/v2/loc/MORecord/en_US/;/wxd/v2/MORecord/en_US/;wxd/v2/DFRecord/en_US/%29/{0}?api={1}";

        /// <summary>
        /// The station Id file
        /// </summary>
        private const string StationIdFile = "Resources/WeatherStationId.txt";

        /// <summary>
        /// The HTTP client
        /// </summary>
        private readonly HttpClient m_Client = new HttpClient();

        /// <summary>
        /// The IoC container
        /// </summary>
        private readonly Container m_Container;

        /// <summary>
        /// The mutex
        /// </summary>
        private readonly object m_Lock = new object();

        /// <summary>
        /// The timer
        /// </summary>
        private readonly Timer m_Timer;

        /// <summary>
        /// The indicator whether the object is disposed or not
        /// </summary>
        private bool m_IsDisposed = false;

        /// <summary>
        /// Initializes a new instance of <see cref="WeatherProvider"/>
        /// </summary>
        /// <param name="container">The IoC container</param>
        public WeatherProvider(Container container)
        {
            m_Container = container;

            m_Timer = new Timer(HandleTimer, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        /// <summary>
        /// Disposes the instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                m_Client.Dispose();
            }
            m_IsDisposed = true;
        }

        /// <summary>
        /// Timer callback to retrieve forecast data
        /// </summary>
        /// <param name="state"></param>
        private async void HandleTimer(object state)
        {
            var apiKey = File.ReadAllText(ApiKeyFile);
            var stationId = File.ReadAllText(StationIdFile);

            ForecastData forecast = ForecastData.Default;

            try
            {
                var forecastUri = string.Format(ForecastUri, stationId, apiKey);
                var forecastData = await m_Client.GetStringAsync(forecastUri);
                forecast = ParseForecast(forecastData);
            }
            catch (Exception)
            {
                throw;
            }

            DispatcherHelper.RunOnUIThread(() =>
            {
                var viewModel = m_Container.ResolveNamed<MainViewModel>(MainViewModel.Name);
                if (viewModel != null)
                {
                    viewModel.ForecastIcon = new BitmapImage(new Uri(forecast.Icon));
                    viewModel.ForecastTemperature = forecast.Temperature;
                    viewModel.ForecastWeather = forecast.Weather;
                    viewModel.ForecastCity = forecast.CityName;
                }
            });
        }

        /// <summary>
        /// Parses the retrieved forecast data
        /// </summary>
        /// <param name="forecastDataString">The retrieved forecast data</param>
        /// <returns>A new instance of <see cref="ForecastData"/></returns>
        private ForecastData ParseForecast(string forecastDataString)
        {
            try
            {
                var forecastData = JArray.Parse(forecastDataString);
                if (forecastData.Count >= 3 && forecastData.First.Value<int>("status") == 200)
                {
                    var cityName = forecastData.SelectToken("[0].doc.cityNm").Value<string>();
                    var token = forecastData.SelectToken("[1].doc.MOData");
                    var temp = token.Value<int>("tmpC");
                    var weather = token.Value<string>("wx");
                    var iconName = token.Value<string>("sky");

                    return new ForecastData
                    {
                        CityName = cityName,
                        Temperature = temp,
                        Weather = weather,
                        Icon = string.Format("ms-appx:///Resources/Icon/{0}.png", iconName)
                    };
                }
            }
            catch (Exception)
            {
            }

            return ForecastData.Default;
        }

        /// <summary>
        /// Object containing all needed data of a forecast
        /// </summary>
        private class ForecastData
        {
            /// <summary>
            /// The default data to be set if no forecast is available
            /// </summary>
            public static readonly ForecastData Default = new ForecastData
            {
                CityName = "not available",
                Temperature = 0,
                Weather = "not available",
                Icon = DefaultForecastIcon
            };

            /// <summary>
            /// Gets or sets the city name
            /// </summary>
            public string CityName { get; set; }

            /// <summary>
            /// Gets or sets the icon
            /// </summary>
            public string Icon { get; set; }

            /// <summary>
            /// Gets or sets the temperature
            /// </summary>
            public int Temperature { get; set; }

            /// <summary>
            /// Gets or sets the weather
            /// </summary>
            public string Weather { get; set; }
        }
    }
}