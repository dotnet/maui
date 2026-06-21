#nullable disable
using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
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

			if (shell.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft)
			{
				var positionX = shellWidth - openPixels;

				if (behavior == FlyoutBehavior.Locked)
				{
					// In RTL (ForceRightToLeft), iOS mirrors the coordinate system so that x=0 is
					// visually on the right. Setting positionX = shellWidth places the flyout's
					// leading edge at the right boundary of the shell content area, which renders
					// at the left (leading) edge of the screen in the mirrored coordinate space.
					// This correctly anchors the locked flyout to the trailing side in RTL layout.
					positionX = shellWidth;
				}

				flyout.Frame = new CGRect(positionX, 0, flyoutWidth, flyoutHeight);
			}
			else
			{
				flyout.Frame = new CGRect(-openLimit + openPixels, 0, flyoutWidth, flyoutHeight);
			}
		}
	}
}
