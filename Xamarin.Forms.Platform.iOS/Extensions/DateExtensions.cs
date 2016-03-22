using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
#if __UNIFIED__
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

#else
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public static class DateExtensions
	{
		public static DateTime ToDateTime(this NSDate date)
		{
			return new DateTime(2001, 1, 1, 0, 0, 0).AddSeconds(date.SecondsSinceReferenceDate);
		}

		public static NSDate ToNSDate(this DateTime date)
		{
			return NSDate.FromTimeIntervalSinceReferenceDate((date - new DateTime(2001, 1, 1, 0, 0, 0)).TotalSeconds);
		}
	}
}