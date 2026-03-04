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
			UpdateBackButtonVisibility(platformToolbar, toolbar);
		}

		public static void UpdateTitleIcon(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			_ = toolbar?.Handler?.MauiContext ?? throw new ArgumentNullException(nameof(toolbar.Handler.MauiContext));
			toolbar.TitleIcon.LoadImage(toolbar.Handler.MauiContext, (result) =>
			{
				if (result != null)
				{
					platformToolbar.TitleIconImageSource = result.Value;
					toolbar.Handler.UpdateValue(nameof(Toolbar.IconColor));
				}
				else
					platformToolbar.TitleIconImageSource = null;
			});
		}

		public static void UpdateBackButton(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.IsBackEnabled =
				toolbar.BackButtonEnabled && toolbar.BackButtonVisible;

			UpdateBackButtonVisibility(platformToolbar, toolbar);

			toolbar.Handler?.UpdateValue(nameof(Toolbar.BarBackground));
		}

		public static void UpdateBarBackground(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.SetBarBackground(toolbar.BarBackground?.ToBrush());
		}

		public static void UpdateTitleView(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			_ = toolbar.Handler?.MauiContext ?? throw new ArgumentNullException(nameof(toolbar.Handler.MauiContext));

			platformToolbar.TitleView = toolbar.TitleView?.ToPlatform(toolbar.Handler.MauiContext);

			if (toolbar.TitleView is IView view)
			{
				platformToolbar.TitleViewMargin = view.Margin.ToPlatform();
			}
			else
			{
				platformToolbar.TitleViewMargin = new UI.Xaml.Thickness(0);
			}
		}

		public static void UpdateIconColor(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.IconColor = toolbar.IconColor;
		}

		public static void UpdateTitle(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.Title = toolbar.Title;
		}

		public static void UpdateBarTextColor(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.SetBarTextColor(toolbar.BarTextColor?.ToPlatform());
		}

		public static void UpdateToolbarDynamicOverflowEnabled(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			if (platformToolbar.CommandBar == null)
				return;

			platformToolbar.CommandBar.IsDynamicOverflowEnabled = toolbar.DynamicOverflowEnabled;
		}

		private static void UpdateBackButtonVisibility(MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.IsBackButtonVisible =
				toolbar.BackButtonVisible
					? NavigationViewBackButtonVisible.Visible
					: NavigationViewBackButtonVisible.Collapsed;
		}
	}
}
