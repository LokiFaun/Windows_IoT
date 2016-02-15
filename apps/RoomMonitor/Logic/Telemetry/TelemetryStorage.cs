namespace Dashboard.Logic.Telemetry
{
    using Dashboard.Extensions;
    using Dashboard.Model;

    using SQLite.Net;
    using SQLite.Net.Interop;
    using SQLite.Net.Platform.WinRT;

    using System;
    using System.Collections.Generic;
    using System.IO;

    using Windows.Storage;

    /// <summary>
    /// Stores the temperature and pressure measurements over time
    /// </summary>
    internal class TelemetryStorage : SQLiteConnection
    {
        /// <summary>
        /// The number of days per month
        /// </summary>
        private const double DaysPerMonth = 31;

        /// <summary>
        /// The number of days per week
        /// </summary>
        private const double DaysPerWeek = 7;

        /// <summary>
        /// The number of days per year
        /// </summary>
        private const double DaysPerYear = 365;

        /// <summary>
        /// The name of the database file
        /// </summary>
        private readonly static string DatebaseFile = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db.sqlite");

        /// <summary>
        /// The IoC container
        /// </summary>
        private readonly Container m_Container;

        /// <summary>
        /// The <see cref="TimeSpan"/> of one month
        /// </summary>
        private readonly TimeSpan OneMonth = TimeSpan.FromDays(DaysPerMonth);

        /// <summary>
        /// The <see cref="TimeSpan"/> of one week
        /// </summary>
        private readonly TimeSpan OneWeek = TimeSpan.FromDays(DaysPerWeek);

        /// <summary>
        /// The <see cref="TimeSpan"/> of one year
        /// </summary>
        private readonly TimeSpan OneYear = TimeSpan.FromDays(DaysPerYear);

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryStorage"/>
        /// </summary>
        /// <param name="container">The IoC container</param>
        public TelemetryStorage(Container container)
            : base(new SQLitePlatformWinRT(), DatebaseFile, SQLiteOpenFlags.ReadWrite)
        {
            m_Container = container;

            CreateTable<Temperature>();
            CreateTable<Pressure>();
        }

        /// <summary>
        /// Adds a new pressure measurement to the repository
        /// </summary>
        /// <param name="item">The pressure measurement</param>
        public void AddPressure(Pressure item)
        {
            Insert(item, typeof(Pressure));
        }

        /// <summary>
        /// Adds a new temperature measurement to the repository
        /// </summary>
        /// <param name="item">The temperature measurement</param>
        public void AddTemperature(Temperature item)
        {
            Insert(item, typeof(Temperature));
        }

        /// <summary>
        /// Gets all pressure measurements of the last month
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Pressure"/></returns>
        public IEnumerable<Pressure> GetPressureLastMonth() =>
            Table<Pressure>()
            .Where(x => x.Measurement > DateTime.Now.Subtract(OneMonth))
            .OrderByDescending(x => x.Measurement);

        /// <summary>
        /// Gets all pressure measurements of the last week
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Pressure"/></returns>
        public IEnumerable<Pressure> GetPressureLastWeek() =>
            Table<Pressure>()
            .Where(x => x.Measurement > DateTime.Now.Subtract(OneWeek))
            .OrderByDescending(x => x.Measurement);

        /// <summary>
        /// Gets all pressure measurements of the last year
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Pressure"/></returns>
        public IEnumerable<Pressure> GetPressureLastYear() =>
            Table<Pressure>()
            .Where(x => x.Measurement > DateTime.Now.Subtract(OneYear))
            .OrderByDescending(x => x.Measurement);

        /// <summary>
        /// Gets all temperature measurements of the last month
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Temperature"/></returns>
        public IEnumerable<Temperature> GetTemperatureLastMonth() =>
            Table<Temperature>()
            .Where(x => x.Measurement > DateTime.Now.Subtract(OneMonth))
            .OrderByDescending(x => x.Measurement);

        /// <summary>
        /// Gets all temperature measurements of the last week
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Temperature"/></returns>
        public IEnumerable<Temperature> GetTemperatureLastWeek() =>
            Table<Temperature>()
            .Where(x => x.Measurement > DateTime.Now.Subtract(OneWeek))
            .OrderByDescending(x => x.Measurement);

        /// <summary>
        /// Gets all temperature measurements of the last year
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Temperature"/></returns>
        public IEnumerable<Temperature> GetTemperatureLastYear() =>
            Table<Temperature>()
            .Where(x => x.Measurement > DateTime.Now.Subtract(OneYear))
            .OrderByDescending(x => x.Measurement);

        /// <summary>
        /// Gets all pressure measurements of today
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Pressure"/></returns>
        public IEnumerable<Pressure> GetTodaysPressure() =>
            Table<Pressure>()
            .Where(x => x.Measurement.IsToday())
            .OrderByDescending(x => x.Measurement);

        /// <summary>
        /// Gets all temperature measurements of today
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Temperature"/></returns>
        public IEnumerable<Temperature> GetTodaysTemperature() =>
            Table<Temperature>()
            .Where(x => x.Measurement.IsToday())
            .OrderByDescending(x => x.Measurement);
    }
}