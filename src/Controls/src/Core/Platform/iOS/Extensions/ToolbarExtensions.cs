using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ToolbarExtensions
	{
		public static void UpdateTitleArea(this UINavigationController navigationController, Toolbar toolbar)
		{
			ImageSource titleIcon = toolbar.TitleIcon;
			var titleView = toolbar.TitleView;

			// ? UpdateLeftBarButtonItem() ?

			ClearTitleViewContainer(navigationController);
			if (titleIcon == null || titleIcon.IsEmpty && titleView == null)
			{
				return;
			}

			if (toolbar.NavigationController == null)
			{
				throw new InvalidOperationException("NavigationController is null.");
			}
			NavigationTitleAreaContainer titleViewContainer = new NavigationTitleAreaContainer((View)titleView, navigationController.NavigationBar);

			UpdateTitleImage(titleViewContainer, titleIcon);
			navigationController.NavigationItem.TitleView = titleViewContainer;
		}

		public static void UpdateBarBackground(this UINavigationBar navigationBar, Toolbar toolbar)
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

		public static void UpdateBackButtonTitle(this UINavigationController navigationController, Toolbar toolbar)
		{
			var viewController = navigationController?.TopViewController;
			if (viewController == null)
			{
				return;
			}

			var backButtonText = toolbar.BackButtonTitle;
			if (backButtonText is null)
			{
				viewController.NavigationItem.BackBarButtonItem = null;
				return;
			}

			viewController.NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = backButtonText, Style = UIBarButtonItemStyle.Plain };
		}

		public static void UpdateBarTextColor(this UINavigationBar navigationBar, Toolbar toolbar)
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

		public static void UpdateToolbarItems(this UINavigationController navigationController, Toolbar toolbar)
		{
			if (navigationController.TopViewController == null)
			{
				return;
			}

			var navigationItem = navigationController.TopViewController.NavigationItem;

			if (navigationItem.RightBarButtonItems != null)
			{
				for (var i = 0; i < navigationItem.RightBarButtonItems.Length; i++)
				{
					navigationItem.RightBarButtonItems[i].Dispose();
				}
			}

			var toolbarItems = navigationController.TopViewController.ToolbarItems;
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

			primaries?.Reverse();

			navigationController!.TopViewController.NavigationItem.SetRightBarButtonItems(primaries == null ? [] : primaries.ToArray(),
				false);
			navigationController.TopViewController.ToolbarItems = secondaries == null ? [] : secondaries.ToArray();
		}

		public static void UpdateBackButtonVisibility(this UINavigationController navigationController, Toolbar toolbar)
		{
			if (navigationController == null)
			{
				throw new NullReferenceException("NavigationController is null.");
			}

			var navigationItem = navigationController.TopViewController?.NavigationItem;

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

		static void ClearTitleViewContainer(UINavigationController navigationController)
		{
			var navigationItem = navigationController?.TopViewController?.NavigationItem;
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
