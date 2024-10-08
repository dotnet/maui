using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class TabbedViewExtensions
	{
		internal static void DisableiOS18ToolbarTabs(
			this UITabBarController tabBarController)
		{
			// Should apply to iOS and Catalyst
            if (OperatingSystem.IsMacCatalystVersionAtLeast(18,0,-1) || //https://github.com/xamarin/xamarin-macios/issues/21390
				OperatingSystem.IsIOSVersionAtLeast(18,0))
            {
				tabBarController.TraitOverrides.HorizontalSizeClass = UIUserInterfaceSizeClass.Compact;
                tabBarController.Mode = UITabBarControllerMode.TabSidebar;
                tabBarController.Sidebar.Hidden = true;
                tabBarController.TabBarHidden = true;
            }
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos15.0")]
		internal static void UpdateiOS15TabBarAppearance(
			this UITabBar tabBar,
			ref UITabBarAppearance _tabBarAppearance,
			UIColor? defaultBarColor,
			UIColor? defaultBarTextColor,
			Color? selectedTabColor,
			Color? unselectedTabColor,
			Color? barBackgroundColor,
			Color? selectedBarTextColor,
			Color? unSelectedBarTextColor)
		{
			if (_tabBarAppearance == null)
			{
				_tabBarAppearance = new UITabBarAppearance();
				_tabBarAppearance.ConfigureWithDefaultBackground();
			}

			var effectiveBarColor = (barBackgroundColor == null) ? defaultBarColor : barBackgroundColor.ToPlatform();
			// Set BarBackgroundColor
			if (effectiveBarColor != null)
			{
				_tabBarAppearance.BackgroundColor = effectiveBarColor;
			}

			// Set BarTextColor

			var effectiveSelectedBarTextColor = (selectedBarTextColor == null) ? defaultBarTextColor : selectedBarTextColor.ToPlatform();
			var effectiveUnselectedBarTextColor = (unSelectedBarTextColor == null) ? defaultBarTextColor : unSelectedBarTextColor.ToPlatform();

			// Update colors for all variations of the appearance to also make it work for iPads, etc. which use different layouts for the tabbar
			// Also, set ParagraphStyle explicitly. This seems to be an iOS bug. If we don't do this, tab titles will be truncat...

			// Set SelectedTabColor
			if (selectedTabColor is not null)
			{
				var foregroundColor = selectedTabColor.ToPlatform();
				var titleColor = effectiveSelectedBarTextColor ?? foregroundColor;

				_tabBarAppearance.StackedLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.StackedLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.InlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.InlineLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.IconColor = foregroundColor;
			}
			else
			{
				var foregroundColor = UITabBar.Appearance.TintColor;
				var titleColor = effectiveSelectedBarTextColor ?? foregroundColor;
				_tabBarAppearance.StackedLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.StackedLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.InlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.InlineLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.IconColor = foregroundColor;
			}

			// Set UnselectedTabColor
			if (unselectedTabColor is not null)
			{
				var foregroundColor = unselectedTabColor.ToPlatform();
				var titleColor = effectiveUnselectedBarTextColor ?? foregroundColor;
				_tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.StackedLayoutAppearance.Normal.IconColor = foregroundColor;

				_tabBarAppearance.InlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.InlineLayoutAppearance.Normal.IconColor = foregroundColor;

				_tabBarAppearance.CompactInlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.CompactInlineLayoutAppearance.Normal.IconColor = foregroundColor;
			}
			else
			{
				var foreground = UITabBar.Appearance.TintColor;
				var titleColor = effectiveUnselectedBarTextColor ?? foreground;
				_tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.StackedLayoutAppearance.Normal.IconColor = foreground;

				_tabBarAppearance.InlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.InlineLayoutAppearance.Normal.IconColor = foreground;

				_tabBarAppearance.CompactInlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = titleColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.CompactInlineLayoutAppearance.Normal.IconColor = foreground;
			}

			// Set the TabBarAppearance
			tabBar.StandardAppearance = tabBar.ScrollEdgeAppearance = _tabBarAppearance;
		}
	}
}
