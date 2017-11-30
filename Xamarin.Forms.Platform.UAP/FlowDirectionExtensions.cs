using Windows.UI.Xaml;
using WFlowDirection = Windows.UI.Xaml.FlowDirection;
using WTextAlignment = Windows.UI.Xaml.TextAlignment;
using Windows.UI.Xaml.Controls;


namespace Xamarin.Forms.Platform.UWP
{
	internal static class FlowDirectionExtensions
	{
		internal static void UpdateFlowDirection(this FrameworkElement control, IVisualElementController controller)
		{
			if (controller == null || control == null)
				return;

			if (controller.EffectiveFlowDirection.IsRightToLeft())
				control.FlowDirection = WFlowDirection.RightToLeft;
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
				control.FlowDirection = WFlowDirection.LeftToRight;
		}

		internal static void UpdateTextAlignment(this TextBox control, IVisualElementController controller)
		{
			if (controller == null || control == null)
				return;

			if (controller.EffectiveFlowDirection.IsRightToLeft())
				control.TextAlignment = WTextAlignment.Right;
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
				control.TextAlignment = WTextAlignment.Left;
		}
	}
}
