using Android.Widget;
using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal static class FlowDirectionExtensions
	{
		internal static void UpdateFlowDirection(this AView view, IVisualElementController controller)
		{
			if (view == null || controller == null || (int)Forms.SdkInt < 17)
				return;

			if (controller is IView v)
			{
				view.UpdateFlowDirection(v);
				return;
			}

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