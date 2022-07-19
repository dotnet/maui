using System;

namespace Microsoft.Maui.Platform
{
	public static class DateTimeExtensions
	{
		public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime)
		{
			return dateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime  
				? DateTimeOffset.MinValue
				: new DateTimeOffset(dateTime);
		}
	}
}