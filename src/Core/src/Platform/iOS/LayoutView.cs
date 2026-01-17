using UIKit;

namespace Microsoft.Maui.Platform
{
	public class LayoutView : MauiView
	{
		public override void SubviewAdded(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.SubviewAdded(uiview);
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.WillRemoveSubview(uiview);
		}
	}
}