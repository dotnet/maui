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
		public static void UpdateIsVisible(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.Visibility = (toolbar.IsVisible) ? UI.Xaml.Visibility.Visible : UI.Xaml.Visibility.Collapsed;
		}

		public static void UpdateTitleIcon(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			_ = toolbar?.Handler?.MauiContext ?? throw new ArgumentNullException(nameof(toolbar.Handler.MauiContext));
			toolbar.TitleIcon.LoadImage(toolbar.Handler.MauiContext, (result) =>
			{
				if (result != null)
				{
					MauiToolbar.TitleIconImageSource = result.Value;
					toolbar.Handler.UpdateValue(nameof(Toolbar.IconColor));
				}
				else
					MauiToolbar.TitleIconImageSource = null;
			});
		}

		public static void UpdateBackButton(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			MauiToolbar.IsBackEnabled =
				toolbar.BackButtonEnabled && toolbar.BackButtonVisible;

			MauiToolbar.IsBackButtonVisible = (toolbar.BackButtonVisible) ? NavigationViewBackButtonVisible.Visible : NavigationViewBackButtonVisible.Collapsed;

			toolbar.Handler?.UpdateValue(nameof(Toolbar.BarBackground));
		}

		public static void UpdateBarBackground(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.Background = toolbar.BarBackground?.ToBrush();
		}

		public static void UpdateTitleView(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			_ = toolbar.Handler?.MauiContext ?? throw new ArgumentNullException(nameof(toolbar.Handler.MauiContext));

			MauiToolbar.TitleView = toolbar.TitleView?.ToPlatform(toolbar.Handler.MauiContext);

			if (toolbar.TitleView is IView view)
			{
				MauiToolbar.TitleViewMargin = view.Margin.ToPlatform();
			}
			else
			{
				MauiToolbar.TitleViewMargin = new UI.Xaml.Thickness(0);
			}
		}

		public static void UpdateIconColor(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.IconColor = toolbar.IconColor;
		}

		public static void UpdateTitle(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			MauiToolbar.Title = toolbar.Title;
		}

		public static void UpdateBarTextColor(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.SetBarTextColor(toolbar.BarTextColor?.ToPlatform());
		}

		public static void UpdateToolbarDynamicOverflowEnabled(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			if (MauiToolbar.CommandBar == null)
				return;

			MauiToolbar.CommandBar.IsDynamicOverflowEnabled = toolbar.DynamicOverflowEnabled;
		}
	}
}
