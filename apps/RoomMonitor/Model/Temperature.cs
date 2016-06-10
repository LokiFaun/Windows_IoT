using SQLite.Net.Attributes;
using System;

namespace Dashboard.Model
{
    /// <summary>
    /// Represents the temperature at a specific measurement time
    /// </summary>
    [Table("Temperatures")]
    internal class Temperature
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Temperature"/>
        /// </summary>
        /// <param name="value">The temperature value in Celsius</param>
        public Temperature(double value)
        {
            Value = value;
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
        /// The temperature value at the specified time
        /// </summary>
        public double Value { get; set; }
    }
}