#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ToolbarExtensions
	{
		public static void UpdateIsVisible(this WindowHeader nativeToolbar, Toolbar toolbar)
		{
			nativeToolbar.Visibility = (toolbar.IsVisible) ? UI.Xaml.Visibility.Visible : UI.Xaml.Visibility.Collapsed;
		}

		public static void UpdateTitleIcon(this WindowHeader nativeToolbar, Toolbar toolbar)
		{
			_ = toolbar?.Handler?.MauiContext ?? throw new ArgumentNullException(nameof(toolbar.Handler.MauiContext));
			toolbar.TitleIcon.LoadImage(toolbar.Handler.MauiContext, (result) =>
			{
				if (result != null)
				{
					nativeToolbar.TitleIconImageSource = result.Value;
					toolbar.Handler.UpdateValue(nameof(Toolbar.IconColor));
				}
				else
					nativeToolbar.TitleIconImageSource = null;
			});
		}

		public static void UpdateBackButton(this WindowHeader nativeToolbar, Toolbar toolbar)
		{
			nativeToolbar
				.IsBackButtonVisible = (toolbar.BackButtonVisible) ? NavigationViewBackButtonVisible.Visible : NavigationViewBackButtonVisible.Collapsed;

			toolbar.Handler?.UpdateValue(nameof(Toolbar.BarBackground));
		}

		public static void UpdateBarBackgroundColor(this WindowHeader nativeToolbar, Toolbar toolbar)
		{
			UpdateBarBackground(nativeToolbar, toolbar);
		}

		public static void UpdateBarBackground(this WindowHeader nativeToolbar, Toolbar toolbar)
		{
			var barBackground = toolbar.BarBackground?.ToBrush() ?? 
				toolbar.BarBackgroundColor?.ToNative();

			nativeToolbar.Background = barBackground;
		}

		public static void UpdateTitleView(this WindowHeader nativeToolbar, Toolbar toolbar)
		{
			_ = toolbar.Handler?.MauiContext ?? throw new ArgumentNullException(nameof(toolbar.Handler.MauiContext));

			nativeToolbar.TitleView = toolbar.TitleView?.ToPlatform(toolbar.Handler.MauiContext);
		}

		public static void UpdateIconColor(this WindowHeader nativeToolbar, Toolbar toolbar)
		{
			// This property wasn't wired up in Controls
		}

		public static void UpdateTitle(this WindowHeader nativeToolbar, Toolbar toolbar)
		{
			nativeToolbar.Title = toolbar.Title;
		}

		public static void UpdateBarTextColor(this WindowHeader nativeToolbar, Toolbar toolbar)
		{
			if (toolbar.BarTextColor != null)
				nativeToolbar.TitleColor = toolbar.BarTextColor.ToNative();
		}

		public static void UpdateToolbarDynamicOverflowEnabled(this WindowHeader nativeToolbar, Toolbar toolbar)
		{
			if (nativeToolbar.CommandBar == null)
				return;

			nativeToolbar.CommandBar.IsDynamicOverflowEnabled = toolbar.DynamicOverflowEnabled;
		}
	}
}
