using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility;

internal class UIContainerViewContainer : UIView
{
	public override void LayoutSubviews()
	{
		base.LayoutSubviews();

		var subviews = Subviews;
		for (int i = 0; i < subviews.Length; i++)
		{
			if (subviews[i] is UIContainerView containerView)
			{
				containerView.NotifyMeasureInvalidated();
			}
		}
	}
}