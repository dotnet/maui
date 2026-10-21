#nullable disable
using System;
using CoreGraphics;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class NavigationPageExtensions
	{
		public static void UpdatePrefersLargeTitles(this UINavigationController platformView, NavigationPage navigationPage)
		{
			if (platformView.NavigationBar is null)
			{
				return;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
			{
				platformView.NavigationBar.PrefersLargeTitles = navigationPage.OnThisPlatform().PrefersLargeTitles();
			}
		}

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

		internal static void SetTransparentNavigationBar(this UINavigationBar navigationBar)
		{
			navigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
			navigationBar.ShadowImage = new UIImage();
			navigationBar.BackgroundColor = UIColor.Clear;
			navigationBar.BarTintColor = UIColor.Clear;
		}

		private static bool IsFiniteRect(CGRect rect) =>
			double.IsFinite((double)rect.Left) &&
			double.IsFinite((double)rect.Top) &&
			double.IsFinite((double)rect.Right) &&
			double.IsFinite((double)rect.Bottom) &&
			rect.Width >= 0 &&
			rect.Height >= 0;
	}
}