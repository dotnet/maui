using Microsoft.Maui.Graphics;
using UIKit;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui
{
	public static partial class ViewHandlerExtensions
	{
		internal static Size GetDesiredSizeFromHandler(this IViewHandler viewHandler, double widthConstraint, double heightConstraint)
		{
			var virtualView = viewHandler.VirtualView;
			var platformView = viewHandler.ToPlatform();

			if (platformView == null || virtualView == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			var sizeThatFits = platformView.SizeThatFits(new CoreGraphics.CGSize((float)widthConstraint, (float)heightConstraint));

			var size = new Size(
				sizeThatFits.Width == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Width,
				sizeThatFits.Height == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Height);

			if (double.IsInfinity(size.Width) || double.IsInfinity(size.Height))
			{
				platformView.SizeToFit();
				size = new Size(platformView.Frame.Width, platformView.Frame.Height);
			}

			var finalWidth = ResolveConstraints(size.Width, virtualView.Width, virtualView.MinimumWidth, virtualView.MaximumWidth);
			var finalHeight = ResolveConstraints(size.Height, virtualView.Height, virtualView.MinimumHeight, virtualView.MaximumHeight);

			return new Size(finalWidth, finalHeight);
		}

		internal static void PlatformArrangeHandler(this IViewHandler viewHandler, Rect rect)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView == null)
				return;

			// We set Center and Bounds rather than Frame because Frame is undefined if the CALayer's transform is 
			// anything other than the identity (https://developer.apple.com/documentation/uikit/uiview/1622459-transform)
			platformView.Center = new CoreGraphics.CGPoint(rect.Center.X, rect.Center.Y);

			// The position of Bounds is usually (0,0), but in some cases (e.g., UIScrollView) it's the content offset.
			// So just leave it a whatever value iOS thinks it should be.
			platformView.Bounds = new CoreGraphics.CGRect(platformView.Bounds.X, platformView.Bounds.Y, rect.Width, rect.Height);

			viewHandler.Invoke(nameof(IView.Frame), rect);
		}

		static double ResolveConstraints(double measured, double exact, double min, double max)
		{
			var resolved = measured;

			min = ResolveMinimum(min);

			if (IsExplicitSet(exact))
			{
				// If an exact value has been specified, try to use that
				resolved = exact;
			}

			if (resolved > max)
			{
				// Apply the max value constraint (if any)
				// If the exact value is in conflict with the max value, the max value should win
				resolved = max;
			}

			if (resolved < min)
			{
				// Apply the min value constraint (if any)
				// If the exact or max value is in conflict with the min value, the min value should win
				resolved = min;
			}

			return resolved;
		}
	}
}
