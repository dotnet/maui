using Microsoft.UI.Xaml;
using WFlowDirection = Microsoft.UI.Xaml.FlowDirection;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
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
