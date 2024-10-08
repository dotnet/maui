using System;
using Android.Widget;
using AGravityFlags = Android.Views.GravityFlags;

namespace Microsoft.Maui.Platform
{
	public static class TextAlignmentExtensions
	{
		// These mask are used to ignore the previous vertical alignment settings
		const AGravityFlags HorizontalGravityMask = AGravityFlags.CenterHorizontal | AGravityFlags.End | AGravityFlags.Start;
		const AGravityFlags VerticalGravityMask = AGravityFlags.Top | AGravityFlags.Bottom | AGravityFlags.CenterVertical;

		internal static void UpdateHorizontalAlignment(this EditText view, TextAlignment alignment, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			view.Gravity = (view.Gravity & ~HorizontalGravityMask) | alignment.ToHorizontalGravityFlags() | orMask;
		}

		public static void UpdateVerticalAlignment(this EditText view, TextAlignment alignment, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			view.Gravity = (view.Gravity & ~VerticalGravityMask) | alignment.ToVerticalGravityFlags() | orMask;
		}

		public static void UpdateVerticalAlignment(this TextView view, TextAlignment alignment, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			view.Gravity = (view.Gravity & ~VerticalGravityMask) | alignment.ToVerticalGravityFlags() | orMask;
		}

		[Obsolete("Use UpdateHorizontalAlignment and/or UpdateVerticalAlignment instead")] // Nothing currently calls this method in our code
		public static void UpdateTextAlignment(this EditText view, TextAlignment horizontal, TextAlignment vertical)
		{
			if (view.Context != null && !Rtl.IsSupported)
			{
				view.Gravity = vertical.ToVerticalGravityFlags() | horizontal.ToHorizontalGravityFlags();
			}
			else
			{
				view.TextAlignment = horizontal.ToTextAlignment();
				view.Gravity = vertical.ToVerticalGravityFlags();
			}
		}
	}
}