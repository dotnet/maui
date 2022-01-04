using Android.OS;
using Android.Widget;
using AGravityFlags = Android.Views.GravityFlags;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[PortHandler]
	internal static class TextAlignmentExtensions
	{
		internal static void UpdateHorizontalAlignment(this EditText view, TextAlignment alignment, bool hasRtlSupport, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			if (!hasRtlSupport)
				view.Gravity = alignment.ToHorizontalGravityFlags() | orMask;
			else
				view.TextAlignment = alignment.ToTextAlignment();
		}

		internal static void UpdateVerticalAlignment(this EditText view, TextAlignment alignment, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			view.Gravity = alignment.ToVerticalGravityFlags() | orMask;
		}

		internal static void UpdateTextAlignment(this EditText view, TextAlignment horizontal, TextAlignment vertical)
		{
			if (!Rtl.IsSupported)
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