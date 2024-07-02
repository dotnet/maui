using UIKit;

namespace Microsoft.Maui.Platform
{
	public class LayoutView : MauiView
	{
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
	}
}