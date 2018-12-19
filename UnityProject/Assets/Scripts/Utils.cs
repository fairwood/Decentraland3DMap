using System;

public static class Utils
{
    public static DateTime UnixTimestampToDateTime(long timestamp)
    {
        return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds(timestamp);
    }
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
    }
}