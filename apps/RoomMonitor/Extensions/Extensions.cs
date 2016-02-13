namespace Dashboard.Extensions
{
    using System;

    /// <summary>
    /// Contains a variety of extension methods
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Checks if the given date is today
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <returns><c>true</c> in case <paramref name="date"/> is today, else <c>false</c></returns>
        public static bool IsToday(this DateTime date)
        {
            var now = DateTime.Now;
            return date.Day == now.Day &&
                date.Month == now.Month &&
                date.Year == now.Year;
        }
    }
}