using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

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
			var brush = paint?.ToNative();

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
			var brush = paint?.ToNative();
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
			var brush = paint?.ToNative();
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
	}
}
