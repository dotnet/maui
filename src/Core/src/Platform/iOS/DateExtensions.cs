using System;
using Foundation;

namespace Microsoft.Maui.Platform;

public static class DateExtensions
{
	internal static DateTime ReferenceDate = new DateTime(2001, 1, 1, 0, 0, 0);

	public static DateTime ToDateTime(this NSDate date)
	{
		return ReferenceDate.AddSeconds(date.SecondsSinceReferenceDate);
	}

	public static NSDate ToNSDate(this DateTime date)
	{
		return NSDate.FromTimeIntervalSinceReferenceDate((date - ReferenceDate).TotalSeconds);
	}
}