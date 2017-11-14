using Android.OS;
using Android.Widget;
using Xamarin.Forms.Internals;
using ALayoutDirection = Android.Views.LayoutDirection;
using AView = Android.Views.View;


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

			if (controller.EffectiveFlowDirection.IsRightToLeft())
				view.LayoutDirection = ALayoutDirection.Rtl;
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
				view.LayoutDirection = ALayoutDirection.Ltr;
		}

		internal static void UpdateHorizontalAlignment(this EditText view, TextAlignment alignment)
		{
			if ((int)Build.VERSION.SdkInt < 17)
				view.Gravity = alignment.ToHorizontalGravityFlags();
			else
				view.TextAlignment = alignment.ToTextAlignment();
		}
	}
}