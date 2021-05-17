using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public interface IShellFlyoutTransition
	{
		void LayoutViews(CGRect bounds, nfloat openPercent, UIView flyout, UIView shell, FlyoutBehavior behavior);
	}
}