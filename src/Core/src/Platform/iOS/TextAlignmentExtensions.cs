using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class TextAlignmentExtensions
	{
		public static UITextAlignment ToPlatformHorizontal(this TextAlignment alignment, IView view)
			=> alignment.ToPlatformHorizontal().AdjustForFlowDirection(view);

		public static UITextAlignment ToPlatformHorizontal(this TextAlignment alignment)
		{
			return alignment switch
			{
				TextAlignment.Center => UITextAlignment.Center,
				TextAlignment.End => UITextAlignment.Right,
				TextAlignment.Start => UITextAlignment.Left,
				_ => UITextAlignment.Left,
			};
		}

		public static UIControlContentVerticalAlignment ToPlatformVertical(this TextAlignment alignment)
		{
			return alignment switch
			{
				TextAlignment.Center => UIControlContentVerticalAlignment.Center,
				TextAlignment.End => UIControlContentVerticalAlignment.Bottom,
				TextAlignment.Start => UIControlContentVerticalAlignment.Top,
				_ => UIControlContentVerticalAlignment.Top,
			};
		}

		public static UITextAlignment AdjustForFlowDirection(this UITextAlignment textAlignment, IView view) 
		{
			if (textAlignment == UITextAlignment.Center)
			{
				// Shortcut center; we don't need to bother checking the flow direction in this case
				return textAlignment;
			}

			var flowDirection = view.GetEffectiveFlowDirection();

			if (flowDirection == FlowDirection.RightToLeft)
			{
				if (textAlignment == UITextAlignment.Left)
				{
					return UITextAlignment.Right;
				}

				if (textAlignment == UITextAlignment.Right)
				{
					return UITextAlignment.Left;
				}
			}

			return textAlignment;
		}
	}
}