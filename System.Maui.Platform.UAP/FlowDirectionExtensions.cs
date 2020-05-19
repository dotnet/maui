using Windows.UI.Xaml;
using WFlowDirection = Windows.UI.Xaml.FlowDirection;

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
	}
}
