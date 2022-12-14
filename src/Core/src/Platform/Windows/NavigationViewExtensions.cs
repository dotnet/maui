using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Platform
{
	public static class NavigationViewExtensions
	{
		public static void UpdateTopNavAreaBackground(this MauiNavigationView navigationView, Paint? paint)
		{
			// Background property is set via {ThemeResource NavigationViewTopPaneBackground} in the Control Template
			// AFAICT you can't modify properties set by ThemeResource at runtime so we have to just update this value directly
			if (paint != null)
				navigationView.TopNavArea?.UpdateBackground(paint, null);
			else if (Application.Current.Resources.TryGetValue("NavigationViewTopPaneBackground", out object value) && value is WBrush brush)
				navigationView.TopNavArea?.UpdateBackground(null, brush);
		}

		public static void UpdateTopNavigationViewItemTextColor(this MauiNavigationView navigationView, Paint? paint)
		{
			var brush = paint?.ToPlatform();

			if (navigationView.TopNavArea != null)
			{
				if (brush is null)
				{
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForeground");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForegroundPointerOver");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForegroundPressed");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemForegroundDisabled");
				}
				else
				{
					navigationView.TopNavArea.Resources["TopNavigationViewItemForeground"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemForegroundPointerOver"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemForegroundPressed"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemForegroundDisabled"] = brush;
				}
			}

			if (navigationView.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					item.Foreground = brush;
				}
			}
		}

		public static void UpdateTopNavigationViewItemBackgroundUnselectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			var brush = paint?.ToPlatform();
			if (navigationView.TopNavArea != null)
			{
				if (brush is null)
				{
					navigationView.TopNavArea.Resources.Remove("NavigationViewItemBackground");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemBackgroundPointerOver");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemBackgroundPressed");
				}
				else
				{
					navigationView.TopNavArea.Resources["NavigationViewItemBackground"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemBackgroundPointerOver"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemBackgroundPressed"] = brush;
				}
			}

			if (navigationView.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					item.UnselectedBackground = brush;
				}
			}
		}

		public static void UpdateTopNavigationViewItemBackgroundSelectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			var brush = paint?.ToPlatform();
			if (navigationView.TopNavArea != null)
			{
				if (brush is null)
				{
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemBackgroundSelected");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemBackgroundSelectedPointerOver");
					navigationView.TopNavArea.Resources.Remove("TopNavigationViewItemBackgroundSelectedPressed");
				}
				else
				{
					navigationView.TopNavArea.Resources["TopNavigationViewItemBackgroundSelected"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemBackgroundSelectedPointerOver"] = brush;
					navigationView.TopNavArea.Resources["TopNavigationViewItemBackgroundSelectedPressed"] = brush;
				}
			}

			if (navigationView.MenuItemsSource is IList<NavigationViewItemViewModel> items)
			{
				foreach (var item in items)
				{
					item.SelectedBackground = brush;
				}
			}
		}

		public static void UpdatePaneBackground(this MauiNavigationView navigationView, Paint? paint)
		{
			var rootSplitView = navigationView.RootSplitView;
			var brush = paint?.ToPlatform();

			if (brush == null)
			{
				object? color = null;
				if (navigationView.IsPaneOpen)
					color = navigationView.Resources["NavigationViewExpandedPaneBackground"];
				else
					color = navigationView.Resources["NavigationViewDefaultPaneBackground"];

				if (rootSplitView != null)
				{
					if (color is WBrush colorBrush)
						rootSplitView.PaneBackground = colorBrush;
					else if (color is global::Windows.UI.Color uiColor)
						rootSplitView.PaneBackground = new WSolidColorBrush(uiColor);
				}
			}
			else
			{
				if (rootSplitView != null)
				{
					rootSplitView.PaneBackground = brush;
				}
			}
		}

		public static void UpdateFlyoutVerticalScrollMode(this MauiNavigationView navigationView, ScrollMode scrollMode)
		{
			var scrollViewer = navigationView.MenuItemsScrollViewer;
			if (scrollViewer != null)
			{
				switch (scrollMode)
				{
					case ScrollMode.Disabled:
						scrollViewer.VerticalScrollMode = WScrollMode.Disabled;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Hidden;
						break;
					case ScrollMode.Enabled:
						scrollViewer.VerticalScrollMode = WScrollMode.Enabled;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Visible;
						break;
					default:
						scrollViewer.VerticalScrollMode = WScrollMode.Auto;
						scrollViewer.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto;
						break;
				}
			}
		}

		public static void UpdateFlyoutBehavior(this MauiNavigationView navigationView, IFlyoutView flyoutView)
		{
			switch (flyoutView.FlyoutBehavior)
			{
				case FlyoutBehavior.Flyout:
					navigationView.IsPaneToggleButtonVisible = true;
					// Workaround for
					// https://github.com/microsoft/microsoft-ui-xaml/issues/6493
					navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
					navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
					break;
				case FlyoutBehavior.Locked:
					navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
					navigationView.IsPaneToggleButtonVisible = false;
					break;
				case FlyoutBehavior.Disabled:
					navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
					navigationView.IsPaneToggleButtonVisible = false;
					navigationView.IsPaneOpen = false;
					break;
			}
		}

		public static void UpdateFlyoutWidth(this MauiNavigationView navigationView, IFlyoutView flyoutView)
		{
			if (flyoutView.FlyoutWidth >= 0)
				navigationView.OpenPaneLength = flyoutView.FlyoutWidth;
			else
				navigationView.OpenPaneLength = 320;
			// At some point this Template Setting is going to show up with a bump to winui
			//handler.PlatformView.OpenPaneLength = handler.PlatformView.TemplateSettings.OpenPaneWidth;
		}
	}
}
