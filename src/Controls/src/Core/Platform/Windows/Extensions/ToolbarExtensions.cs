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
		public static void UpdateIsVisible(this MauiToolbar nativeToolbar, Toolbar toolbar)
		{
			nativeToolbar.Visibility = (toolbar.IsVisible) ? UI.Xaml.Visibility.Visible : UI.Xaml.Visibility.Collapsed;
		}

		public static void UpdateTitleIcon(this MauiToolbar nativeToolbar, Toolbar toolbar)
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

		public static void UpdateBackButton(this MauiToolbar nativeToolbar, Toolbar toolbar)
		{
			nativeToolbar.IsBackEnabled =
				toolbar.BackButtonEnabled && toolbar.BackButtonVisible;

			nativeToolbar
				.IsBackButtonVisible = (toolbar.BackButtonVisible) ? NavigationViewBackButtonVisible.Visible : NavigationViewBackButtonVisible.Collapsed;

			toolbar.Handler?.UpdateValue(nameof(Toolbar.BarBackground));
		}

		public static void UpdateBarBackground(this MauiToolbar nativeToolbar, Toolbar toolbar)
		{
			nativeToolbar.Background = toolbar.BarBackground?.ToBrush();
		}

		public static void UpdateTitleView(this MauiToolbar nativeToolbar, Toolbar toolbar)
		{
			_ = toolbar.Handler?.MauiContext ?? throw new ArgumentNullException(nameof(toolbar.Handler.MauiContext));

			nativeToolbar.TitleView = toolbar.TitleView?.ToPlatform(toolbar.Handler.MauiContext);
		}

		public static void UpdateIconColor(this MauiToolbar nativeToolbar, Toolbar toolbar)
		{
			// This property wasn't wired up in Controls
		}

		public static void UpdateTitle(this MauiToolbar nativeToolbar, Toolbar toolbar)
		{
			nativeToolbar.Title = toolbar.Title;
		}

		public static void UpdateBarTextColor(this MauiToolbar nativeToolbar, Toolbar toolbar)
		{
			if (toolbar.BarTextColor != null)
				nativeToolbar.TitleColor = toolbar.BarTextColor.ToPlatform();
		}

		public static void UpdateToolbarDynamicOverflowEnabled(this MauiToolbar nativeToolbar, Toolbar toolbar)
		{
			if (nativeToolbar.CommandBar == null)
				return;

			nativeToolbar.CommandBar.IsDynamicOverflowEnabled = toolbar.DynamicOverflowEnabled;
		}
	}
}
