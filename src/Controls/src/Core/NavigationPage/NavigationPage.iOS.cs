#nullable disable
using System;
using Microsoft.Maui.Controls.Platform;
using UIKit;
using iOSSpecificNavigationPage = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		public static void MapPrefersLargeTitles(NavigationViewHandler handler, NavigationPage navigationPage) =>
			MapPrefersLargeTitles((INavigationViewHandler)handler, navigationPage);

		public static void MapPrefersLargeTitles(INavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is IPlatformViewHandler nvh && nvh.ViewController is UINavigationController navigationController)
			{
				NavigationPageExtensions.UpdatePrefersLargeTitles(navigationController, navigationPage);
			}
		}

		static void MapBarBackground(NavigationViewHandler handler, NavigationPage navigationPage)
		{
			var navBar = handler.NavigationController?.NavigationBar;

			if (navBar is null)
			{
				return;
			}

			var barBackgroundColor = navigationPage.BarBackgroundColor;
			var barBackground = navigationPage.BarBackground;

			if (barBackground is SolidColorBrush scb)
			{
				barBackgroundColor = scb.Color;
				barBackground = null;
			}

			var navigationBarAppearance = navBar.StandardAppearance;

			if (barBackgroundColor is null && barBackground is null)
			{
				navigationBarAppearance.ConfigureWithOpaqueBackground();
				navigationBarAppearance.BackgroundColor = ColorExtensions.BackgroundColor;
			}
			else if (barBackgroundColor is not null)
			{
				if (barBackgroundColor.Alpha < 1f)
				{
					navigationBarAppearance.ConfigureWithTransparentBackground();
					navBar.Translucent = true;
				}
				else
				{
					navigationBarAppearance.ConfigureWithOpaqueBackground();
					navBar.Translucent = false;
				}

				navigationBarAppearance.BackgroundColor = barBackgroundColor.ToPlatform();
			}

			if (barBackground is not null)
			{
				navigationBarAppearance.BackgroundImage = ((UIView)navBar).GetBackgroundImage(barBackground);
			}

			navBar.CompactAppearance = navigationBarAppearance;
			navBar.StandardAppearance = navigationBarAppearance;
			navBar.ScrollEdgeAppearance = navigationBarAppearance;
		}

		static void MapBarTextColor(NavigationViewHandler handler, NavigationPage navigationPage)
		{
			var navBar = handler.NavigationController?.NavigationBar;

			if (navBar is null)
			{
				return;
			}

			var barTextColor = navigationPage.BarTextColor;

			var globalTitleTextAttributes = UINavigationBar.Appearance.TitleTextAttributes;
			var titleTextAttributes = new UIStringAttributes
			{
				ForegroundColor = barTextColor is null
					? globalTitleTextAttributes?.ForegroundColor
					: barTextColor.ToPlatform(),
				Font = globalTitleTextAttributes?.Font
			};

			var largeTitleTextAttributes = titleTextAttributes;

			if (OperatingSystem.IsIOSVersionAtLeast(11))
			{
				var globalLargeTitleTextAttributes = UINavigationBar.Appearance.LargeTitleTextAttributes;
				largeTitleTextAttributes = new UIStringAttributes
				{
					ForegroundColor = barTextColor is null
						? globalLargeTitleTextAttributes?.ForegroundColor
						: barTextColor.ToPlatform(),
					Font = globalLargeTitleTextAttributes?.Font
				};
			}

			navBar.CompactAppearance.TitleTextAttributes = titleTextAttributes;
			navBar.CompactAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
			navBar.StandardAppearance.TitleTextAttributes = titleTextAttributes;
			navBar.StandardAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
			navBar.ScrollEdgeAppearance.TitleTextAttributes = titleTextAttributes;
			navBar.ScrollEdgeAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;

			navBar.TintColor = barTextColor is null
				? UINavigationBar.Appearance.TintColor
				: barTextColor.ToPlatform();

			// Per-page IconColor overrides TintColor (back arrow + button text)
			var iconColor = navigationPage.CurrentPage is Page current ? GetIconColor(current) : null;

			if (iconColor is not null)
			{
				var statusBarMode = iOSSpecificNavigationPage.GetStatusBarTextColorMode(navigationPage);
				if (statusBarMode != PlatformConfiguration.iOSSpecific.StatusBarTextColorMode.DoNotAdjust)
				{
					navBar.TintColor = iconColor.ToPlatform();
				}
			}

			// iOS 26+ Liquid Glass ignores TintColor for the back button; apply via appearance instead.
			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
			{
				var effectiveColor = iconColor ?? barTextColor;
				var statusBarMode = iOSSpecificNavigationPage.GetStatusBarTextColorMode(navigationPage);
				var useCustomColor = effectiveColor is not null && statusBarMode != PlatformConfiguration.iOSSpecific.StatusBarTextColorMode.DoNotAdjust;

				if (handler.NavigationController?.VisibleViewController?.NavigationItem?.RightBarButtonItems is UIBarButtonItem[] items)
				{
					foreach (var item in items)
					{
						item.TintColor = navBar.TintColor;
					}
				}

				if (useCustomColor)
				{
					var backColor = effectiveColor!.ToPlatform();
					var colorAttributes = Foundation.NSDictionary<Foundation.NSString, Foundation.NSObject>.FromObjectsAndKeys(
						new Foundation.NSObject[] { backColor }, new Foundation.NSString[] { UIStringAttributeKey.ForegroundColor });
					var btnAppearance = new UIBarButtonItemAppearance(UIBarButtonItemStyle.Plain);
					btnAppearance.Normal.TitleTextAttributes = colorAttributes;
					btnAppearance.Highlighted.TitleTextAttributes = colorAttributes;

					UIImage tintedImage = null;
					var backImage = UIImage.GetSystemImage("chevron.backward");

					if (backImage is not null)
					{
						tintedImage = backImage.ApplyTintColor(backColor).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
						navBar.BackIndicatorImage = tintedImage;
						navBar.BackIndicatorTransitionMaskImage = tintedImage;
					}

					// Set BackButtonAppearance and back indicator on each appearance object,
					// then reassign to the nav bar to force UIKit to process the changes.
					// In-place mutation alone is not detected by UIKit on iOS 26 Liquid Glass.
					var compactAppearance = navBar.CompactAppearance;
					compactAppearance.BackButtonAppearance = btnAppearance;
					if (tintedImage is not null)
					{
						compactAppearance.SetBackIndicatorImage(tintedImage, tintedImage);
					}
					navBar.CompactAppearance = compactAppearance;

					var standardAppearance = navBar.StandardAppearance;
					standardAppearance.BackButtonAppearance = btnAppearance;

					if (tintedImage is not null)
					{
						standardAppearance.SetBackIndicatorImage(tintedImage, tintedImage);
					}

					navBar.StandardAppearance = standardAppearance;

					var scrollEdgeAppearance = navBar.ScrollEdgeAppearance;
					scrollEdgeAppearance.BackButtonAppearance = btnAppearance;

					if (tintedImage is not null)
					{
						scrollEdgeAppearance.SetBackIndicatorImage(tintedImage, tintedImage);
					}

					navBar.ScrollEdgeAppearance = scrollEdgeAppearance;
				}
			}

			SetStatusBarStyle(navigationPage);
		}

		static void MapStatusBarTextColorMode(NavigationViewHandler handler, NavigationPage navigationPage)
		{
			SetStatusBarStyle(navigationPage);
		}

		static void SetStatusBarStyle(NavigationPage navigationPage)
		{
			var barTextColor = navigationPage.BarTextColor;
			var statusBarColorMode = iOSSpecificNavigationPage.GetStatusBarTextColorMode(navigationPage);

#pragma warning disable CA1416, CA1422 // 'UIApplication.StatusBarStyle' is unsupported on: 'ios' 9.0 and later
			if (statusBarColorMode == PlatformConfiguration.iOSSpecific.StatusBarTextColorMode.DoNotAdjust || barTextColor?.GetLuminosity() <= 0.5)
			{
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
				UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
			}
#pragma warning restore CA1416, CA1422
		}

		static void MapPrefersHomeIndicatorAutoHidden(NavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is IPlatformViewHandler pvh && pvh.ViewController is UINavigationController navController)
			{
				navController.SetNeedsUpdateOfHomeIndicatorAutoHidden();
			}
		}

		static void MapPrefersStatusBarHidden(NavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is IPlatformViewHandler pvh && pvh.ViewController is UINavigationController navController)
			{
				navController.SetNeedsStatusBarAppearanceUpdate();
			}
		}

		static void MapHideNavigationBarSeparator(NavigationViewHandler handler, NavigationPage navigationPage)
		{
			var navBar = handler.NavigationController?.NavigationBar;

			if (navBar is null)
			{
				return;
			}

			bool shouldHide = iOSSpecificNavigationPage.GetHideNavigationBarSeparator(navigationPage);

			if (shouldHide)
			{
				navBar.CompactAppearance.ShadowColor = UIColor.Clear;
				navBar.StandardAppearance.ShadowColor = UIColor.Clear;
				navBar.ScrollEdgeAppearance.ShadowColor = UIColor.Clear;
			}
			else
			{
				var defaultShadowColor = UIColor.FromRGBA(0, 0, 0, 76);
				navBar.CompactAppearance.ShadowColor = defaultShadowColor;
				navBar.StandardAppearance.ShadowColor = defaultShadowColor;
				navBar.ScrollEdgeAppearance.ShadowColor = defaultShadowColor;
			}
		}
	}
}