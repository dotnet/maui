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
			if (paint is null)
			{
				navigationView.Resources.Remove("TopNavigationViewItemForeground");
				navigationView.Resources.Remove("TopNavigationViewItemForegroundPointerOver");
				navigationView.Resources.Remove("TopNavigationViewItemForegroundPressed");
				navigationView.Resources.Remove("TopNavigationViewItemForegroundDisabled");
			}
			else
			{
				navigationView.Resources["TopNavigationViewItemForeground"] = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemForegroundPointerOver"] = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemForegroundPressed"] = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemForegroundDisabled"] = paint.ToNative();
			}

			navigationView.TopNavArea?.UpdateLayout();
		}

		public static void UpdateTopNavigationViewItemTextSelectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			if (paint is null)
			{
				navigationView.Resources.Remove("TopNavigationViewItemForegroundSelected");
				navigationView.Resources.Remove("TopNavigationViewItemForegroundSelectedPointerOver");
				navigationView.Resources.Remove("TopNavigationViewItemForegroundSelectedPressed");
			}
			else
			{
				navigationView.Resources["TopNavigationViewItemForegroundSelected"] = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemForegroundSelectedPointerOver"] = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemForegroundSelectedPressed"] = paint.ToNative();
			}

			navigationView.TopNavArea?.UpdateLayout();
		}

		public static void UpdateTopNavigationViewItemBackgroundColor(this MauiNavigationView navigationView, Paint? paint)
		{
			if (paint is null)
			{
				navigationView.Resources.Remove("NavigationViewItemBackground");
				navigationView.Resources.Remove("TopNavigationViewItemBackgroundPointerOver");
				navigationView.Resources.Remove("TopNavigationViewItemBackgroundPressed");
			}
			else
			{
				navigationView.Resources["NavigationViewItemBackground"] = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemBackgroundPointerOver"] = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemBackgroundPressed"] = paint.ToNative();
			}
		}

		public static void UpdateTopNavigationViewItemBackgroundSelectedColor(this MauiNavigationView navigationView, Paint? paint)
		{
			if (paint is null)
			{
				navigationView.Resources.Remove("TopNavigationViewItemBackgroundSelected");
				navigationView.Resources.Remove("TopNavigationViewItemBackgroundSelectedPointerOver");
				navigationView.Resources.Remove("TopNavigationViewItemBackgroundSelectedPressed");
			}
			else
			{
				navigationView.Resources["TopNavigationViewItemBackgroundSelected"] = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemBackgroundSelectedPointerOver"] = paint.ToNative();
				navigationView.Resources["TopNavigationViewItemBackgroundSelectedPressed"] = paint.ToNative();
			}
		}
	}
}
