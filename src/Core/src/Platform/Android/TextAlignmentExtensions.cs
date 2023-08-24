using Android.Widget;
using AGravityFlags = Android.Views.GravityFlags;

namespace Microsoft.Maui.Platform
{
	public static class TextAlignmentExtensions
	{
		internal static void UpdateHorizontalAlignment(this EditText view, TextAlignment alignment, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			if (!Rtl.IsSupported)
			{
				// The mask is used to ignore the previous horizontal alignment settings
				var horizontalGravityMask = AGravityFlags.CenterHorizontal | AGravityFlags.End | AGravityFlags.Start;
				view.Gravity = (view.Gravity & ~horizontalGravityMask) | alignment.ToHorizontalGravityFlags() | orMask;
			}
			else
				view.TextAlignment = alignment.ToTextAlignment();
		}

		public static void UpdateVerticalAlignment(this EditText view, TextAlignment alignment, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			// The mask is used to ignore the previous vertical alignment settings
			var verticalGravityMask = AGravityFlags.Top | AGravityFlags.Bottom | AGravityFlags.CenterVertical;
			view.Gravity = (view.Gravity & ~verticalGravityMask) | alignment.ToVerticalGravityFlags() | orMask;
		}

		public static void UpdateVerticalAlignment(this TextView view, TextAlignment alignment, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			// The mask is used to ignore the previous vertical alignment settings
			var verticalGravityMask = AGravityFlags.Top | AGravityFlags.Bottom | AGravityFlags.CenterVertical;
			view.Gravity = (view.Gravity & ~verticalGravityMask) | alignment.ToVerticalGravityFlags() | orMask;
		}

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