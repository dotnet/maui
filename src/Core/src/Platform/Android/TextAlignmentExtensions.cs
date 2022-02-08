using Android.Widget;
using AGravityFlags = Android.Views.GravityFlags;

namespace Microsoft.Maui.Platform
{
	public static class TextAlignmentExtensions
	{
		internal static void UpdateHorizontalAlignment(this EditText view, TextAlignment alignment, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			if (!Rtl.IsSupported)
				view.Gravity = alignment.ToHorizontalGravityFlags() | orMask;
			else
				view.TextAlignment = alignment.ToTextAlignment();
		}

		public static void UpdateVerticalAlignment(this EditText view, TextAlignment alignment, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			view.Gravity = alignment.ToVerticalGravityFlags() | orMask;
		}

		public static void UpdateVerticalAlignment(this TextView view, TextAlignment alignment, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			view.Gravity = alignment.ToVerticalGravityFlags() | orMask;
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