using Android.OS;
using Android.Widget;
using Xamarin.Forms.Internals;
using ALayoutDirection = Android.Views.LayoutDirection;
using AView = Android.Views.View;
using AGravityFlags = Android.Views.GravityFlags;


namespace Xamarin.Forms.Platform.Android
{
	internal static class FlowDirectionExtensions
	{
		internal static FlowDirection ToFlowDirection(this ALayoutDirection direction)
		{
			switch (direction)
			{
				case ALayoutDirection.Ltr:
					return FlowDirection.LeftToRight;
				case ALayoutDirection.Rtl:
					return FlowDirection.RightToLeft;
				default:
					return FlowDirection.MatchParent;
			}
		}

		internal static void UpdateFlowDirection(this AView view, IVisualElementController controller)
		{
			if (view == null || controller == null || (int)Build.VERSION.SdkInt < 17)
				return;

			// if android:targetSdkVersion < 17 setting these has no effect
			if (controller.EffectiveFlowDirection.IsRightToLeft())
				view.LayoutDirection = ALayoutDirection.Rtl;
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
				view.LayoutDirection = ALayoutDirection.Ltr;
		}

		internal static void UpdateHorizontalAlignment(this EditText view, TextAlignment alignment, bool hasRtlSupport, AGravityFlags orMask = AGravityFlags.NoGravity)
		{
			if ((int)Build.VERSION.SdkInt < 17 || !hasRtlSupport)
				view.Gravity = alignment.ToHorizontalGravityFlags() | orMask;
			else
				view.TextAlignment = alignment.ToTextAlignment();
		}
	}
}