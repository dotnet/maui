using System;
using Foundation;

namespace Microsoft.Maui
{
	public static class DateExtensions
	{
		internal static DateTime ReferenceDate = new DateTime(2001, 1, 1, 0, 0, 0);

		public static DateTime ToDateTime(this NSDate date)
		{
			return ReferenceDate.AddSeconds(date.SecondsSinceReferenceDate);
		}
		public static NSDate? ToNSDate(this DateTime? date)
		{
			if (date == null)
				return null;

			return NSDate.FromTimeIntervalSinceReferenceDate((date.Value - ReferenceDate).TotalSeconds);
		}
	}
}