#nullable disable
using System;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class NavigationPageExtensions
	{
		public static void UpdatePrefersLargeTitles(this UINavigationController platformView, NavigationPage navigationPage)
		{
			if (platformView.NavigationBar is null)
				return;

			if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
				platformView.NavigationBar.PrefersLargeTitles = navigationPage.OnThisPlatform().PrefersLargeTitles();
		}

		public static void UpdateIsNavigationBarTranslucent(this UINavigationController platformView, NavigationPage navigationPage)
		{
			if (platformView.NavigationBar is null)
				return;

			platformView.NavigationBar.Translucent = navigationPage.OnThisPlatform().IsNavigationBarTranslucent();
		}

		internal static void SetTransparentNavigationBar(this UINavigationBar navigationBar)
		{
			navigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
			navigationBar.ShadowImage = new UIImage();
			navigationBar.BackgroundColor = UIColor.Clear;
			navigationBar.BarTintColor = UIColor.Clear;
		}
	}
}