using UIKit;
using Microsoft.Maui.Controls.Compatibility.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal static class FlowDirectionExtensions
	{
		internal static FlowDirection ToFlowDirection(this UIUserInterfaceLayoutDirection direction)
		{
			switch (direction)
			{
				case UIUserInterfaceLayoutDirection.LeftToRight:
					return FlowDirection.LeftToRight;
				case UIUserInterfaceLayoutDirection.RightToLeft:
					return FlowDirection.RightToLeft;
				default:
					return FlowDirection.MatchParent;
			}
		}

		internal static bool UpdateFlowDirection(this UIView view, IVisualElementController controller)
		{
			if (controller == null || view == null || !Forms.IsiOS9OrNewer)
				return false;

			UISemanticContentAttribute updateValue = view.SemanticContentAttribute;

			if (controller.EffectiveFlowDirection.IsRightToLeft())
				updateValue = UISemanticContentAttribute.ForceRightToLeft;
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
				updateValue = UISemanticContentAttribute.ForceLeftToRight;

			if (updateValue != view.SemanticContentAttribute)
			{
				view.SemanticContentAttribute = updateValue;
				return true;
			}

			return false;
		}

		internal static void UpdateTextAlignment(this UITextField control, IVisualElementController controller)
		{
			if (controller == null || control == null)
				return;

			if (controller.EffectiveFlowDirection.IsRightToLeft())
			{
				control.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
				control.TextAlignment = UITextAlignment.Right;
			}
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
			{
				control.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
				control.TextAlignment = UITextAlignment.Left;
			}
		}

		internal static void UpdateTextAlignment(this UITextView control, IVisualElementController controller)
		{
			if (controller == null || control == null)
				return;

			if (controller.EffectiveFlowDirection.IsRightToLeft())
				control.TextAlignment = UITextAlignment.Right;
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
				control.TextAlignment = UITextAlignment.Left;
		}
	}
}