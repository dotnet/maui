using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ToolbarExtensions
	{
		internal static void UpdateTitleArea(this UINavigationBar navigationBar, Toolbar toolbar)
		{
			ImageSource titleIcon = toolbar.TitleIcon;
			var titleView = toolbar.TitleView;

			// UpdateLeftBarButtonItem() ?
			//navigationBar.UpdateBackButtonTitle(toolbar);

			ClearTitleViewContainer(toolbar);
			if (titleIcon == null || titleIcon.IsEmpty && titleView == null)
			{
				return;
			}

			if (toolbar.NavigationController == null)
			{
				throw new InvalidOperationException("NavigationController is null.");
			}
			NavigationTitleAreaContainer titleViewContainer = new NavigationTitleAreaContainer((View)titleView, toolbar.NavigationController.NavigationBar);

			UpdateTitleImage(titleViewContainer, titleIcon);
			toolbar.NavigationController.NavigationItem.TitleView = titleViewContainer;
		}

		internal static void UpdateBarBackground(this UINavigationBar navigationBar, Toolbar toolbar)
		{
			var barBackgroundBrush = toolbar.BarBackground;
			Graphics.Color? barBackgroundColor = null;

			// if the brush has a solid color, treat it as a Color, so we can compute the alpha value
			if (barBackgroundBrush is SolidColorBrush scb)
			{
				barBackgroundColor = scb.Color;
				barBackgroundBrush = null;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
			{
				var navigationBarAppearance = navigationBar.StandardAppearance;
				if (barBackgroundColor is null)
				{
					navigationBarAppearance.ConfigureWithOpaqueBackground();
					navigationBarAppearance.BackgroundColor = ColorExtensions.BackgroundColor;
					navigationBar.SetupDefaultNavigationBarAppearance();
				}
				else
				{
					if (barBackgroundColor.Alpha < 1f)
					{
						navigationBarAppearance.ConfigureWithTransparentBackground();
					}
					else
					{
						navigationBarAppearance.ConfigureWithOpaqueBackground();
					}

					navigationBarAppearance.BackgroundColor = barBackgroundColor.ToPlatform();
				}

				if (barBackgroundBrush is not null)
				{
					var backgroundImage = navigationBar.GetBackgroundImage(barBackgroundBrush);

					navigationBarAppearance.BackgroundImage = backgroundImage;
				}

				navigationBar.CompactAppearance = navigationBarAppearance;
				navigationBar.StandardAppearance = navigationBarAppearance;
				navigationBar.ScrollEdgeAppearance = navigationBarAppearance;
			}
			else
			{
				if (barBackgroundColor?.Alpha == 0f)
				{
					navigationBar.SetTransparentNavigationBar();
				}
				else
				{
					// Set navigation bar background color
					navigationBar.BarTintColor = barBackgroundColor == null
						? UINavigationBar.Appearance.BarTintColor
						: barBackgroundColor.ToPlatform();

					var backgroundImage = navigationBar.GetBackgroundImage(barBackgroundBrush);
					navigationBar.SetBackgroundImage(backgroundImage, UIBarMetrics.Default);
				}
			}
		}

		internal static void UpdateBackButtonTitle(this UINavigationBar navigationBar, Toolbar toolbar)
		{
			var viewController = toolbar.NavigationController?.TopViewController;
			if (viewController == null)
			{
				return;
			}

			if (!string.IsNullOrWhiteSpace(toolbar.Title))
			{
				viewController.NavigationItem.Title = toolbar.Title;
				return;
			}

			var backButtonText = toolbar.BackButtonTitle;
			if (string.IsNullOrWhiteSpace(backButtonText))
			{
				viewController.NavigationItem.BackBarButtonItem = null;
				return;
			}

			viewController.NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = backButtonText, Style = UIBarButtonItemStyle.Plain };
		}

		internal static void SetupDefaultNavigationBarAppearance(this UINavigationBar navBar)
		{
			if (!(OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13)))
			{
				return;
			}

			// We will use UINavigationBar.Appearance to infer settings that
			// were already set to the navigation bar in older versions of
			// iOS.
			UINavigationBarAppearance navAppearance = navBar.StandardAppearance;

			if (navAppearance.BackgroundColor == null)
			{
				UIColor? backgroundColor = navBar.BarTintColor;

				navBar.StandardAppearance.BackgroundColor = backgroundColor;

				if (navBar.ScrollEdgeAppearance != null)
				{
					navBar.ScrollEdgeAppearance.BackgroundColor = backgroundColor;
				}

				if (navBar.CompactAppearance != null)
				{
					navBar.CompactAppearance.BackgroundColor = backgroundColor;
				}
			}

			if (navAppearance.BackgroundImage == null)
			{
				UIImage backgroundImage = navBar.GetBackgroundImage(UIBarMetrics.Default);

				navBar.StandardAppearance.BackgroundImage = backgroundImage;

				if (navBar.ScrollEdgeAppearance != null)
				{
					navBar.ScrollEdgeAppearance.BackgroundImage = backgroundImage;
				}

				if (navBar.CompactAppearance != null)
				{
					navBar.CompactAppearance.BackgroundImage = backgroundImage;
				}
			}

			if (navAppearance.ShadowImage == null)
			{
				UIImage? shadowImage = navBar.ShadowImage;
				UIColor clearColor = UIColor.Clear;

				navBar.StandardAppearance.ShadowImage = shadowImage;

				if (navBar.ScrollEdgeAppearance != null)
				{
					navBar.ScrollEdgeAppearance.ShadowImage = shadowImage;
				}

				if (navBar.CompactAppearance != null)
				{
					navBar.CompactAppearance.ShadowImage = shadowImage;
				}

				if (shadowImage != null && shadowImage.Size == SizeF.Empty)
				{
					navBar.StandardAppearance.ShadowColor = clearColor;

					if (navBar.ScrollEdgeAppearance != null)
					{
						navBar.ScrollEdgeAppearance.ShadowColor = clearColor;
					}

					if (navBar.CompactAppearance != null)
					{
						navBar.CompactAppearance.ShadowColor = clearColor;
					}
				}
			}

			UIImage? backIndicatorImage = navBar.BackIndicatorImage;
			UIImage? backIndicatorTransitionMaskImage = navBar.BackIndicatorTransitionMaskImage;

			if (backIndicatorImage != null && backIndicatorImage.Size == SizeF.Empty)
			{
				backIndicatorImage = GetEmptyBackIndicatorImage();
			}

			if (backIndicatorTransitionMaskImage != null && backIndicatorTransitionMaskImage.Size == SizeF.Empty)
			{
				backIndicatorTransitionMaskImage = GetEmptyBackIndicatorImage();
			}

			navBar.CompactAppearance?.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
			navBar.StandardAppearance.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
			navBar.ScrollEdgeAppearance?.SetBackIndicatorImage(backIndicatorImage, backIndicatorTransitionMaskImage);
		}

		internal static UIImage GetEmptyBackIndicatorImage()
		{
			var rect = RectangleF.Empty;
			SizeF size = rect.Size;

			UIGraphics.BeginImageContext(size);
			CGContext? context = UIGraphics.GetCurrentContext();
			context?.SetFillColor(1, 1, 1, 0);
			context?.FillRect(rect);

			UIImage? empty = UIGraphics.GetImageFromCurrentImageContext();
			context?.Dispose();

			return empty;
		}

		internal static ParentViewController? GetParentViewController(this UINavigationBar navigationBar)
		{
			var viewControllers = navigationBar.GetNavigationController()?.ViewControllers;
			if (!viewControllers?.Any() ?? true)
			{
				return null;
			}

			var parentViewController = viewControllers!.Last() as ParentViewController;
			return parentViewController;
		}

		internal static void UpdateBarTextColor(this UINavigationBar navigationBar, Toolbar toolbar)
		{
			var barTextColor = toolbar.BarTextColor;

			// Determine new title text attributes via global static data
			var globalTitleTextAttributes = UINavigationBar.Appearance.TitleTextAttributes;

			// using the object initializer gave us a NullReferenceException, but this way works, oddly
			var titleTextAttributes = new UIStringAttributes();
			titleTextAttributes.ForegroundColor = barTextColor == null ? globalTitleTextAttributes?.ForegroundColor : barTextColor.ToPlatform();

			// Determine new large title text attributes via global static data
			var globalLargeTitleTextAttributes = UINavigationBar.Appearance.LargeTitleTextAttributes;
			var largeTitleTextAttributes = new UIStringAttributes
			{
				ForegroundColor = barTextColor == null ? globalLargeTitleTextAttributes?.ForegroundColor : barTextColor.ToPlatform(),
				Font = globalLargeTitleTextAttributes?.Font
			};
			
			if (OperatingSystem.IsIOSVersionAtLeast(13))
			{
				if (navigationBar.CompactAppearance != null)
				{
					navigationBar.CompactAppearance.TitleTextAttributes = titleTextAttributes;
					navigationBar.CompactAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
				}
				
				navigationBar.StandardAppearance.TitleTextAttributes = titleTextAttributes;
				navigationBar.StandardAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
				
				if (navigationBar.ScrollEdgeAppearance != null)
				{
					navigationBar.ScrollEdgeAppearance.TitleTextAttributes = titleTextAttributes;
					navigationBar.ScrollEdgeAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
				}
			}
			else
			{
				navigationBar.TitleTextAttributes = titleTextAttributes;
				navigationBar.LargeTitleTextAttributes = largeTitleTextAttributes;
			}

			// set Tint color (i.e. Back Button arrow and Text)
			Graphics.Color? iconColor = null;
			if (toolbar is NavigationPageToolbar navPageToolbar)
			{
				if (navPageToolbar.CurrentNavigationPage != null)
				{
					iconColor = navPageToolbar.IconColor;
				}

				iconColor ??= barTextColor;

				navigationBar.TintColor = iconColor == null || navPageToolbar.CurrentNavigationPage.OnThisPlatform().GetStatusBarTextColorMode() ==
					StatusBarTextColorMode.DoNotAdjust
						? UINavigationBar.Appearance.TintColor
						: iconColor.ToPlatform();
			}

			if (iconColor == null)
			{
				navigationBar.TintColor = UINavigationBar.Appearance.TintColor;
			}
		}

		internal static void UpdateToolbarItems(this UINavigationBar navigationBar, Toolbar toolbar)
		{
			if (toolbar.NavigationController == null || toolbar.NavigationController.TopViewController == null)
			{
				return;
			}

			var navigationItem = toolbar.NavigationController.TopViewController.NavigationItem;

			if (navigationItem.RightBarButtonItems != null)
			{
				for (var i = 0; i < navigationItem.RightBarButtonItems.Length; i++)
				{
					navigationItem.RightBarButtonItems[i].Dispose();
				}
			}

			var toolbarItems = toolbar.NavigationController.TopViewController.ToolbarItems;
			if (toolbarItems != null)
			{
				for (var i = 0; i < toolbarItems.Length; i++)
				{
					toolbarItems[i].Dispose();
				}
			}

			List<UIBarButtonItem>? primaries = null;
			List<UIBarButtonItem>? secondaries = null;
			foreach (var item in toolbar.ToolbarItems)
			{
				if (item.Order == ToolbarItemOrder.Secondary)
				{
					(secondaries ??= new List<UIBarButtonItem>()).Add(item.ToUIBarButtonItem(true));
				}
				else
				{
					(primaries ??= new List<UIBarButtonItem>()).Add(item.ToUIBarButtonItem());
				}
			}

			if (primaries != null)
			{
				primaries.Reverse();
			}

			toolbar.NavigationController!.TopViewController.NavigationItem.SetRightBarButtonItems(primaries == null ? [] : primaries.ToArray(),
				false);
			toolbar.NavigationController
				.TopViewController.ToolbarItems = secondaries == null ? [] : secondaries.ToArray();

			toolbar.NavigationController.UpdateNavigationBarVisibility(toolbar.IsVisible, true); // TODO: check that we need this call at all 
		}

		internal static void UpdateBackButtonVisibility(this UINavigationBar navigationBar, Toolbar toolbar)
		{
			if (toolbar.NavigationController == null)
			{
				throw new NullReferenceException("NavigationController is null.");
			}

			var navigationItem = toolbar.NavigationController.TopViewController?.NavigationItem;

			if (navigationItem == null)
			{
				return;
			}

			if (navigationItem.HidesBackButton == !toolbar.BackButtonVisible)
			{
				return;
			}

			navigationItem.HidesBackButton = !toolbar.BackButtonVisible;
		}

		// TODO: StatusBarTextColorModeProperty is on NavigationPage, look at NavigationRenderer and how it uses this
//		static void SetStatusBarStyle()
//		{
//			var barTextColor = NavPage.BarTextColor;
//			var statusBarColorMode = NavPage.OnThisPlatform().GetStatusBarTextColorMode();

//#pragma warning disable CA1416, CA1422 // TODO:   'UIApplication.StatusBarStyle' is unsupported on: 'ios' 9.0 and later
//			if (statusBarColorMode == StatusBarTextColorMode.DoNotAdjust || barTextColor?.GetLuminosity() <= 0.5)
//			{
//				// Use dark text color for status bar
//				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
//				{
//					UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.DarkContent;
//				}
//				else
//				{
//					UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;
//				}
//			}
//			else
//			{
//				// Use light text color for status bar
//				UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
//			}
//#pragma warning restore CA1416, CA1422
//		}

		static void ClearTitleViewContainer(Toolbar toolbar)
		{
			var navigationItem = toolbar.NavigationController?.TopViewController?.NavigationItem;
			if (navigationItem == null)
			{
				return;
			}

			if (navigationItem.TitleView != null && navigationItem.TitleView is NavigationTitleAreaContainer titleViewContainer)
			{
				titleViewContainer.Dispose();
				navigationItem.TitleView = null;
			}
		}

		static void UpdateTitleImage(NavigationTitleAreaContainer titleViewContainer, ImageSource titleIcon)
		{
			if (titleIcon.IsEmpty)
			{
				titleViewContainer.Icon = null;
			}
			else
			{
				var context = titleIcon.FindMauiContext();
				if (context == null)
				{
					return;
				}

				titleIcon.LoadImage(context, result =>
				{
					var image = result?.Value;
					try
					{
						titleViewContainer.Icon = new UIImageView(image);
					}
					catch
					{
						//UIImage ctor throws on file not found if MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure is true;
					}
				});
			}
		}
	}
}
