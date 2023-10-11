using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class LayoutView : MauiView
	{
		bool _userInteractionEnabled;

		public override void SubviewAdded(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.SubviewAdded(uiview);
			Superview?.SetNeedsLayout();
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.WillRemoveSubview(uiview);
			Superview?.SetNeedsLayout();
		}

		public override UIView HitTest(CGPoint point, UIEvent? uievent)
		{
			var result = base.HitTest(point, uievent);

			if (result is null)
				return null!;

			if (!_userInteractionEnabled && this.Equals(result))
			{
				// If user interaction is disabled (IOW, if the corresponding Layout is InputTransparent),
				// then we exclude the LayoutView itself from hit testing. But it's children are valid
				// hit testing targets.

				return null!;
			}

			if (!result.UserInteractionEnabled)
			{
				// If the child also has user interaction disabled (IOW the child is InputTransparent),
				// then we also want to exclude it from the hit testing.

				return null!;
			}

			if (result is LayoutView layoutView && !layoutView.UserInteractionEnabledOverride)
			{
				// If the child is a layout then we need to check the UserInteractionEnabledOverride
				// since layouts always have user interaction enabled.

				return null!;
			}

			return result;
		}

		internal bool UserInteractionEnabledOverride => _userInteractionEnabled;

		public override bool UserInteractionEnabled
		{
			get => base.UserInteractionEnabled;
			set
			{
				// We leave the base UIE value true no matter what, so that hit testing will find children
				// of the LayoutView. But we track the intended value so we can use it during hit testing
				// to ignore the LayoutView itself, if necessary.

				base.UserInteractionEnabled = true;
				_userInteractionEnabled = value;
			}
		}
	}
}