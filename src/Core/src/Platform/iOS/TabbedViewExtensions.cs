using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class TabbedViewExtensions
	{
		internal static void UpdateiOS15TabBarAppearance(
			this UITabBar tabBar,
			ref UITabBarAppearance _tabBarAppearance,
			UIColor? defaultBarColor,
			UIColor? defaultBarTextColor,
			Color? selectedTabColor,
			Color? unselectedTabColor,
			Color? barBackgroundColor,
			Color? barTextColor)
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

			var effectiveBarTextColor = (barTextColor == null) ? defaultBarTextColor : barTextColor.ToPlatform();
			if (effectiveBarTextColor != null)
			{
				_tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes
				{
					ForegroundColor = effectiveBarTextColor
				};
			}

			// Update colors for all variations of the appearance to also make it work for iPads, etc. which use different layouts for the tabbar
			// Also, set ParagraphStyle explicitly. This seems to be an iOS bug. If we don't do this, tab titles will be truncat...

			// Set SelectedTabColor
			if (selectedTabColor != null)
			{
				var foregroundColor = selectedTabColor.ToPlatform();
				_tabBarAppearance.StackedLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foregroundColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.StackedLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.InlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foregroundColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.InlineLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foregroundColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.IconColor = foregroundColor;
			}
			else
			{
				var foregroundColor = UITabBar.Appearance.TintColor;
				_tabBarAppearance.StackedLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foregroundColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.StackedLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.InlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foregroundColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.InlineLayoutAppearance.Selected.IconColor = foregroundColor;

				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foregroundColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.CompactInlineLayoutAppearance.Selected.IconColor = foregroundColor;
			}

			// Set UnselectedTabColor
			if (unselectedTabColor != null)
			{
				var foregroundColor = unselectedTabColor.ToPlatform();
				_tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foregroundColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.StackedLayoutAppearance.Normal.IconColor = foregroundColor;

				_tabBarAppearance.InlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foregroundColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.InlineLayoutAppearance.Normal.IconColor = foregroundColor;

				_tabBarAppearance.CompactInlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foregroundColor, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.CompactInlineLayoutAppearance.Normal.IconColor = foregroundColor;
			}
			else
			{
				var foreground = UITabBar.Appearance.TintColor;
				_tabBarAppearance.StackedLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foreground, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.StackedLayoutAppearance.Normal.IconColor = foreground;

				_tabBarAppearance.InlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foreground, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.InlineLayoutAppearance.Normal.IconColor = foreground;

				_tabBarAppearance.CompactInlineLayoutAppearance.Normal.TitleTextAttributes = new UIStringAttributes { ForegroundColor = foreground, ParagraphStyle = NSParagraphStyle.Default };
				_tabBarAppearance.CompactInlineLayoutAppearance.Normal.IconColor = foreground;
			}

			// Set the TabBarAppearance
			tabBar.StandardAppearance = tabBar.ScrollEdgeAppearance = _tabBarAppearance;
		}
	}
}
