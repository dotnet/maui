using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiUIWindow : UIWindow
	{
		public MauiUIWindow() { }

#pragma warning disable CA1416 // UIWindow(windowScene) is only supported on: ios 13.0 and later
		public MauiUIWindow(UIWindowScene uIWindowScene) : base(uIWindowScene) { }
#pragma warning restore CA1416

		public override void DidUpdateFocus(UIFocusUpdateContext context, UIFocusAnimationCoordinator coordinator)
		{
			NSNotificationCenter.DefaultCenter.PostNotificationName(nameof(UIView.DidUpdateFocus), context.NextFocusedView);

			NSNotificationCenter.DefaultCenter.PostNotificationName(nameof(UIView.DidUpdateFocus), context.PreviouslyFocusedView);

			base.DidUpdateFocus(context, coordinator);
		}
	}
}

