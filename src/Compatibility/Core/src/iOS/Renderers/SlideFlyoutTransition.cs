using CoreGraphics;
using System;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class SlideFlyoutTransition : IShellFlyoutTransition
	{
		internal double Height { get; private set; } = -1d;
		internal double Width { get; private set; } = -1d;

		public virtual bool UpdateFlyoutSize(double height, double width)
		{
			if (Height != height ||
				Width != width)
			{
				Height = height;
				Width = width;
				return true;
			}

			return false;
		}

		public virtual void LayoutViews(CGRect bounds, nfloat openPercent, UIView flyout, UIView shell, FlyoutBehavior behavior)
		{
			if (behavior == FlyoutBehavior.Locked)
				openPercent = 1;

			nfloat flyoutHeight;
			nfloat flyoutWidth;

			if (Width != -1d)
				flyoutWidth = (nfloat)Width;
			else if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				flyoutWidth = 320;
			else
				flyoutWidth = (nfloat)(Math.Min(bounds.Width, bounds.Height) * 0.8);

			if (Height == -1d)
				flyoutHeight = bounds.Height;	
			else
				flyoutHeight = (nfloat)Height;

			nfloat openLimit = flyoutWidth;
			nfloat openPixels = openLimit * openPercent;

			if (behavior == FlyoutBehavior.Locked)
				shell.Frame = new CGRect(bounds.X + flyoutWidth, bounds.Y, bounds.Width - flyoutWidth, flyoutHeight);
			else
				shell.Frame = bounds;

			var shellWidth = shell.Frame.Width;

			if(shell.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft)
			{
				var positionY = shellWidth - openPixels;
				flyout.Frame = new CGRect(positionY, 0, flyoutWidth, flyoutHeight);
			}
			else
			{
				flyout.Frame = new CGRect(-openLimit + openPixels, 0, flyoutWidth, flyoutHeight);
			}
		}
	}
}
