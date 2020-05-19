using Android.Widget;
using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;
using AView = Android.Views.View;

namespace System.Maui.Platform.Android
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
			if (view == null || controller == null || (int)System.Maui.Maui.SdkInt < 17)
				return;

			// if android:targetSdkVersion < 17 setting these has no effect
			if (controller.EffectiveFlowDirection.IsRightToLeft())
			{
				view.LayoutDirection = ALayoutDirection.Rtl;

				if (view is TextView textView)
					textView.TextDirection = ATextDirection.Rtl;
			}
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
			{
				view.LayoutDirection = ALayoutDirection.Ltr;

				if (view is TextView textView)
					textView.TextDirection = ATextDirection.Ltr;
			}
		}
	}
}