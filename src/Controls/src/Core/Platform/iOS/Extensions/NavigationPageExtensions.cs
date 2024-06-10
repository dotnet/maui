#nullable disable
using System;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
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

		internal static void SetTransparentNavigationBar (this UINavigationBar navigationBar)
		{
			navigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
			navigationBar.ShadowImage = new UIImage();
			navigationBar.BackgroundColor = UIColor.Clear;
			navigationBar.BarTintColor = UIColor.Clear;
		}

		public static void SetStatusBarStyle(this UINavigationController platformView, StatusBarTextColorMode statusBarColorMode, Color barTextColor)
		{
#pragma warning disable CA1416, CA1422 // TODO:   'UIApplication.StatusBarStyle' is unsupported on: 'ios' 9.0 and later
			if (statusBarColorMode == StatusBarTextColorMode.DoNotAdjust || barTextColor?.GetLuminosity() <= 0.5)
			{
				// Use dark text color for status bar
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
				{
					UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.DarkContent;
				}
				else
				{
					UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;
				}
			}
			else
			{
				// Use light text color for status bar
				UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
			}
#pragma warning restore CA1416, CA1422
		}
	}
}