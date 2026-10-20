using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility;

internal static class NavigationRendererExtensions
{
	internal static CGRect GetNavigationContentBounds(this UIViewController viewController)
	{
		var view = viewController.View!;
		var bounds = view.Bounds;

		if (!OperatingSystem.IsIOSVersionAtLeast(26) ||
			(viewController.EdgesForExtendedLayout & UIRectEdge.Top) != 0 ||
			viewController.NavigationController is not { } navigationController ||
			navigationController.NavigationBarHidden)
		{
			return bounds;
		}

		var navigationBar = navigationController.NavigationBar;
		if (navigationBar.Hidden ||
			navigationBar.Translucent ||
			view.Window is null ||
			navigationBar.Window != view.Window)
		{
			return bounds;
		}

		var barFrame = navigationBar.ConvertRectToView(navigationBar.Bounds, view);
		if (!IsFiniteRect(bounds) ||
			!IsFiniteRect(barFrame) ||
			barFrame.Right <= bounds.Left ||
			barFrame.Left >= bounds.Right ||
			barFrame.Bottom <= bounds.Top ||
			barFrame.Top >= bounds.Bottom)
		{
			return bounds;
		}

		var topOverlap = (nfloat)Math.Clamp((double)(barFrame.Bottom - bounds.Top), 0, (double)bounds.Height);
		return new CGRect(bounds.X, bounds.Y + topOverlap, bounds.Width, bounds.Height - topOverlap);
	}

	private static bool IsFiniteRect(CGRect rect) =>
		double.IsFinite((double)rect.Left) &&
		double.IsFinite((double)rect.Top) &&
		double.IsFinite((double)rect.Right) &&
		double.IsFinite((double)rect.Bottom) &&
		rect.Width >= 0 &&
		rect.Height >= 0;
}
