using Dashboard.Logic;
using Dashboard.Logic.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dashboard.ViewModel
{
    internal class SensorsViewModel : ViewModel
    {
        /// <summary>
        /// The name of the <see cref="SensorsViewModel"/>.
        /// </summary>
        public const string Name = "Sensors";

        /// <summary>
        /// The IoC container
        /// </summary>
        private readonly Container m_Container;

        /// <summary>
        /// The pressure values
        /// </summary>
        private IEnumerable<NameValueItem> m_PressureValues;

        /// <summary>
        /// The temperature values
        /// </summary>
        private IEnumerable<NameValueItem> m_TemperatureValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorsViewModel"/>
        /// </summary>
        /// <param name="container">The IoC container</param>
        public SensorsViewModel(Container container)
        {
            m_Container = container;
        }

        /// <summary>
        /// Gets or sets the pressure values
        /// </summary>
        public IEnumerable<NameValueItem> PressureValues
        {
            get { return m_PressureValues; }
            set
            {
                m_PressureValues = value;
                NotifyPropertyChanged(nameof(PressureValues));
            }
        }

        /// <summary>
        /// Gets or sets the temperature values
        /// </summary>
        public IEnumerable<NameValueItem> TemperatureValues
        {
            get { return m_TemperatureValues; }
            set
            {
                m_TemperatureValues = value;
                NotifyPropertyChanged(nameof(TemperatureValues));
            }
        }

        /// <summary>
        /// Updates the data to display
        /// </summary>
        public void Update()
        {
            //using (var db = new TelemetryStorage(m_Container))
            //{
            //    TemperatureValues = db.GetTemperatureLastWeek().Select(x => new NameValueItem(x.Measurement, x.Value));
            //    PressureValues = db.GetPressureLastWeek().Select(x => new NameValueItem(x.Measurement, x.Value));
            //}
        }

        /// <summary>
        /// Internal class to store telemetry representation data
        /// </summary>
        public class NameValueItem
        {
            /// <summary>
            /// Initializes a new instance of <see cref="NameValueItem"/>
            /// </summary>
            /// <param name="measurement">The measurement time</param>
            /// <param name="value">The measurement value</param>
            public NameValueItem(DateTime measurement, double value)
            {
                Measurement = measurement;
                Value = value;
            }

            public DateTime Measurement { get; set; }
            public double Value { get; set; }
        }
    }
}