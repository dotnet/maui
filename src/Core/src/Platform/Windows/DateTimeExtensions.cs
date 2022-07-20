using System;

namespace Microsoft.Maui.Platform
{
	// TODO NET7 MAKE PUBLIC
	internal static class DateTimeExtensions
	{
		public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime)
		{
			return dateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime
				? DateTimeOffset.MinValue
				: new DateTimeOffset(dateTime);
		}
	}
}