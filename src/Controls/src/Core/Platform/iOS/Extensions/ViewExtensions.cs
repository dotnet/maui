using UIKit;

namespace Microsoft.Maui.Controls.Platform;

static class UIViewExtensions
{
	public static void AdjustBounds(UIView? childView, UIView? superView)
	{
		if (childView is null || superView is null)
		{
			return;
		}
			
		childView.Frame = superView.Bounds;
	}
}