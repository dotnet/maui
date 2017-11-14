using AppKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.MacOS
{
	internal static class FlowDirectionExtensions
	{
		internal static FlowDirection ToFlowDirection(this NSApplicationLayoutDirection direction)
		{
			switch (direction)
			{
				case NSApplicationLayoutDirection.LeftToRight:
					return FlowDirection.LeftToRight;
				case NSApplicationLayoutDirection.RightToLeft:
					return FlowDirection.RightToLeft;
				default:
					return FlowDirection.MatchParent;
			}
		}

		internal static void UpdateFlowDirection(this NSView view, IVisualElementController controller)
		{
			if (controller == null || view == null)
				return;

			if (controller.EffectiveFlowDirection.IsRightToLeft())
				view.UserInterfaceLayoutDirection = NSUserInterfaceLayoutDirection.RightToLeft;
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
				view.UserInterfaceLayoutDirection = NSUserInterfaceLayoutDirection.LeftToRight;
		}

		internal static void UpdateFlowDirection(this NSTextField control, IVisualElementController controller)
		{
			if (controller == null || control == null)
				return;

			if (controller.EffectiveFlowDirection.IsRightToLeft())
			{
				control.Alignment = NSTextAlignment.Right;
			}
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
			{
				control.Alignment = NSTextAlignment.Left;
			}
		}
	}
}