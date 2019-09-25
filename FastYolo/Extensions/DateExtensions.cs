using System;
using System.Globalization;

namespace FastYolo.Extensions
{
	/// <summary>
	///   Allows to write out date values as structured iso date strings and parses iso or english dates
	/// </summary>
	public static class DateExtensions
	{
		private static readonly TimeSpan UtcOffset =
			StackTraceExtensions.StartedFromNCrunchOrForcedToUseMockResolver
				? TimeSpan.Zero
				: TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

		private static readonly CultureInfo EnglishCultureInfo = new CultureInfo("en-US", false);

		/// <summary>
		///   DateTime.UtcNow is faster than DateTime.Now, see http://stackoverflow.com/questions/1561791
		/// </summary>
		public static DateTime FastDateTimeNow => DateTime.UtcNow + UtcOffset;

		public static string GetIsoDateTime(this DateTime dateTime)
		{
			return GetIsoDate(dateTime) + " " + GetIsoTime(dateTime);
		}

		public static string GetIsoDate(this DateTime date)
		{
			return date.ToString("yyyy-MM-dd");
		}

		public static string GetIsoTime(this DateTime time)
		{
			return time.ToString("HH:mm:ss");
		}

		public static DateTime Parse(string dateString)
		{
			if (string.IsNullOrEmpty(dateString))
				return DateTime.MinValue;
			return DateTime.TryParse(dateString, EnglishCultureInfo, DateTimeStyles.AssumeLocal,
				out var result)
				? result
				: FastDateTimeNow;
		}

		public static bool IsDateNewerByOneSecond(DateTime newerDate, DateTime olderDate)
		{
			return (newerDate - olderDate).TotalSeconds > 1;
		}

		/// <summary>
		///   Converts unix time expressed as seconds since 1/1/1970 to a proper DateTime.
		///   http://stackoverflow.com/questions/2883576/how-do-you-convert-epoch-time-in-c
		/// </summary>
		public static DateTime FromUnixTimeStampInSeconds(long unixTimeStamp)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp);
		}

		public static DateTime FromUnixTimeStampInMilliseconds(long unixTimeStampMs)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unixTimeStampMs);
		}

		public static DateTime RoundUp(this DateTime dateTime, TimeSpan roundBy)
		{
			return new DateTime((dateTime.Ticks + roundBy.Ticks - 1) / roundBy.Ticks * roundBy.Ticks);
		}
	}
}