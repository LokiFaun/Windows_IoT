using SQLite.Net.Attributes;
using System;

namespace Dashboard.Model
{
    /// <summary>
    /// Represents the pressure at a specific measurement time
    /// </summary>
    [Table("Pressures")]
    internal class Pressure
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Pressure"/>
        /// </summary>
        /// <param name="value">The pressure value in hPa</param>
        public Pressure(double value)
        {
            Value = value;
            Measurement = DateTime.Now;
        }

        /// <summary>
        /// Unique id within the database
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Date and time of the measurement
        /// </summary>
        public DateTime Measurement { get; set; }

        /// <summary>
        /// The pressure value at the specified time
        /// </summary>
        public double Value { get; set; }
    }
}