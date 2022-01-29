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
		static IEnumerable<NavigationViewItem> GetNavigationViewItems(this MauiNavigationView navigationView)
		{
			if (navigationView.MenuItems?.Count > 0)
			{
				foreach (var menuItem in navigationView.MenuItems)
				{
					if (menuItem is NavigationViewItem item)
						yield return item;
				}
			}
			else if (navigationView.MenuItemsSource != null && navigationView.TopNavMenuItemsHost != null)
			{
				var itemCount = navigationView.TopNavMenuItemsHost.ItemsSourceView.Count;
				for (int i = 0; i < itemCount; i++)
				{
					UI.Xaml.UIElement uIElement = navigationView.TopNavMenuItemsHost.TryGetElement(i);

					if (uIElement is NavigationViewItem item)
						yield return item;
				}

			}
		}

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
			if (paint is null)
			{
				navigationView.Resources.Remove("TopNavigationViewItemForeground");
				navigationView.Resources.Remove("TopNavigationViewItemForegroundPointerOver");
				navigationView.Resources.Remove("TopNavigationViewItemForegroundPressed");
				navigationView.Resources.Remove("TopNavigationViewItemForegroundDisabled");

				foreach (var menuItem in navigationView.GetNavigationViewItems())
				{
					menuItem.ClearValue(NavigationViewItem.ForegroundProperty);
				}
			}
			else
			{
				var brush = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemForeground"] = brush;
				navigationView.Resources["TopNavigationViewItemForegroundPointerOver"] = brush;
				navigationView.Resources["TopNavigationViewItemForegroundPressed"] = brush;
				navigationView.Resources["TopNavigationViewItemForegroundDisabled"] = brush;

				foreach (var menuItem in navigationView.GetNavigationViewItems())
				{
					menuItem.Foreground = brush;
				}
			}
		}

		public static void UpdateTopNavigationViewItemBackgroundUnselectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			if (paint is null)
			{
				//navigationView.Resources.Remove("NavigationViewItemBackground");
				//navigationView.Resources.Remove("TopNavigationViewItemBackgroundPointerOver");
				//navigationView.Resources.Remove("TopNavigationViewItemBackgroundPressed");
				foreach (var item in navigationView.GetNavigationViewItems())
				{
					if (item.IsSelected)
						continue;

					item.ClearValue(NavigationViewItem.BackgroundProperty);
				}
			}
			else
			{
				var brush = paint.ToNative();
				navigationView.Resources["NavigationViewItemBackground"] = brush;
				navigationView.Resources["TopNavigationViewItemBackgroundPointerOver"] = brush;
				navigationView.Resources["TopNavigationViewItemBackgroundPressed"] = brush;
				foreach (var item in navigationView.GetNavigationViewItems())
				{
					if (item.IsSelected)
						continue;

					item.Background = brush;
				}
			}
		}

		public static void UpdateTopNavigationViewItemBackgroundSelectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			if (paint is null)
			{
				//navigationView.Resources.Remove("TopNavigationViewItemBackgroundSelected");
				//navigationView.Resources.Remove("TopNavigationViewItemBackgroundSelectedPointerOver");
				//navigationView.Resources.Remove("TopNavigationViewItemBackgroundSelectedPressed");

				foreach (var item in navigationView.GetNavigationViewItems())
				{
					if (!item.IsSelected)
						continue;

					// We can't just clear the value because that will set the SelectedColor to what we set the 
					// unselectedcolor to. NavigationViewItem doesn't have a property for UnselectedItem so there's just
					// default and selected
					item.SetApplicationResource("NavigationViewItemBackground", null);
					item.SetApplicationResource("TopNavigationViewItemBackgroundPointerOver", null);
					item.SetApplicationResource("TopNavigationViewItemBackgroundPressed", null);
					item.ClearValue(NavigationViewItem.BackgroundProperty);
					break;
				}
			}
			else
			{
				var brush = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemBackgroundSelected"] = brush;
				navigationView.Resources["TopNavigationViewItemBackgroundSelectedPointerOver"] = brush;
				navigationView.Resources["TopNavigationViewItemBackgroundSelectedPressed"] = brush;
				foreach (var item in navigationView.GetNavigationViewItems())
				{
					if (!item.IsSelected)
						continue;

					item.Background = brush;
					break;
				}
			}
		}
	}
}
