namespace Dashboard.Converter
{
    using System;
    using System.Globalization;
    using Windows.UI.Xaml.Data;

    /// <summary>
    /// Converts the given <see cref="DateTime"/> to a 24 hour formatted time string and back
    /// </summary>
    internal class TimeConverter : IValueConverter
    {
        /// <summary>
        /// Converts the given <see cref="DateTime"/> value
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">The target type to convert to</param>
        /// <param name="parameter">The conversion parameter</param>
        /// <param name="language">The language</param>
        /// <returns>The value as 24 hour formatted string</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.GetType() != typeof(DateTime))
            {
                return value;
            }
            var date = DateTime.Parse(value.ToString());
            return date.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the given value back into <see cref="DateTime"/> object
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">The target type to convert to</param>
        /// <param name="parameter">The conversion parameter</param>
        /// <param name="language">The language</param>
        /// <returns>The <see cref="DateTime"/> represented by the given string</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value.GetType() != typeof(string))
            {
                return value;
            }
            var date = DateTime.Parse(value.ToString());
            return date;
        }
    }
}
