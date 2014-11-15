namespace EyeTracking.App
{
    using System;

    public static class DateTimeExtensions
    {
        public static bool InRange(this DateTime @this, DateTime t, DateTime T)
        {
            return @this < T && @this >= t;
        }

        public static bool InRange(this DateTime @this, DateTime t, TimeSpan ts)
        {
            return @this.InRange(t, t.Add(ts));
        }

        public static double TotalMilliseconds(this DateTime @this)
        {
            return (@this - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}
