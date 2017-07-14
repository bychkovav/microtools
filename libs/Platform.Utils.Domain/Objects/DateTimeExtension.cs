using System;

namespace Platform.Utils.Domain.Objects
{
    public static class DateTimeExtension
    {
        /// <returns>UTC</returns>
        public static DateTime ConvertJavaMiliSecondToDateTime(this long javaMiliSeconds)
        {
            var utcBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime result = utcBaseTime.Add(
                new TimeSpan(javaMiliSeconds*TimeSpan.TicksPerMillisecond));

            return result;
        }

        public static DateTime ConvertJavaMiliSecondToDateTimeLocal(this long javaMiliSeconds)
        {
            var utcBaseTime = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime result = utcBaseTime.Add(
                new TimeSpan(javaMiliSeconds * TimeSpan.TicksPerMillisecond));

            return result;
        }

        public static long UnixTimestampFromDateTime(this DateTime date)
        {
            long unixTimestamp = date.ToUniversalTime().Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks;
            unixTimestamp /= TimeSpan.TicksPerMillisecond;
            return unixTimestamp;
        }

        public static long UnixTimestampFromDateTimeLocal(this DateTime date)
        {
            long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks;
            unixTimestamp /= TimeSpan.TicksPerMillisecond;
            return unixTimestamp;
        }
    }
}