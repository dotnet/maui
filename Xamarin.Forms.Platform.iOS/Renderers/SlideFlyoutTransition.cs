using CoreGraphics;
using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class SlideFlyoutTransition : IShellFlyoutTransition
	{
		public void LayoutViews(CGRect bounds, nfloat openPercent, UIView flyout, UIView shell, FlyoutBehavior behavior)
		{
			if (behavior == FlyoutBehavior.Locked)
				openPercent = 1;

			nfloat flyoutWidth = (nfloat)(Math.Min(bounds.Width, bounds.Height) * 0.8);
			nfloat openLimit = flyoutWidth;
			nfloat openPixels = openLimit * openPercent;

			if (behavior == FlyoutBehavior.Locked)
				shell.Frame = new CGRect(bounds.X + flyoutWidth, bounds.Y, bounds.Width - flyoutWidth, bounds.Height);
			else
				shell.Frame = bounds;

			var shellWidth = shell.Frame.Width;

			if(shell.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft)
			{
				var positionY = shellWidth - openPixels;
				flyout.Frame = new CGRect(positionY, 0, flyoutWidth, bounds.Height);
			}
			else
			{
				flyout.Frame = new CGRect(-openLimit + openPixels, 0, flyoutWidth, bounds.Height);
			}
		}
	}
}