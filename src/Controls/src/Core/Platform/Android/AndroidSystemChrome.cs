#nullable enable
using System;
using System.Runtime.CompilerServices;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Google.Android.Material.AppBar;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using AWindow = Android.Views.Window;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class AndroidSystemChrome
	{
		static readonly ConditionalWeakTable<AppBarLayout, OriginalAppBarBackground> s_originalAppBarBackgrounds = new();

		internal static void UpdateTopChrome(AView? chromeView, Brush? background, Color? foreground = null)
		{
			if (chromeView is null)
			{
				return;
			}

			var appBarLayout = chromeView.GetParentOfType<AppBarLayout>();
			UpdateAppBarBackground(appBarLayout, background);
			UpdateSystemBarAppearance(
				chromeView.Context,
				window: null,
				updateStatusBar: true,
				updateNavigationBar: false,
				statusBarBackgroundColor: GetSolidColor(background),
				statusBarForegroundColor: foreground);
		}

		internal static void UpdateBottomChrome(AView? chromeView, Brush? background, Color? foreground = null)
		{
			if (chromeView is null)
			{
				return;
			}

			UpdateSystemBarAppearance(
				chromeView.Context,
				window: null,
				updateStatusBar: false,
				updateNavigationBar: true,
				navigationBarBackgroundColor: GetSolidColor(background),
				navigationBarForegroundColor: foreground);
		}

		internal static void UpdateWindowChrome(
			Context? context,
			AWindow? window,
			bool updateStatusBar,
			bool updateNavigationBar,
			Paint? background = null,
			Color? foreground = null)
		{
			var backgroundColor = GetSolidColor(background);
			UpdateSystemBarAppearance(
				context,
				window,
				updateStatusBar,
				updateNavigationBar,
				statusBarBackgroundColor: backgroundColor,
				statusBarForegroundColor: foreground,
				navigationBarBackgroundColor: backgroundColor,
				navigationBarForegroundColor: foreground);
		}

		internal static void UpdateWindowChrome(
			Context? context,
			AWindow? window,
			bool updateStatusBar,
			bool updateNavigationBar,
			Brush? statusBarBackground,
			Color? statusBarForeground = null,
			Paint? navigationBarBackground = null,
			Color? navigationBarForeground = null)
		{
			UpdateSystemBarAppearance(
				context,
				window,
				updateStatusBar,
				updateNavigationBar,
				statusBarBackgroundColor: GetSolidColor(statusBarBackground),
				statusBarForegroundColor: statusBarForeground,
				navigationBarBackgroundColor: GetSolidColor(navigationBarBackground),
				navigationBarForegroundColor: navigationBarForeground);
		}

		static void UpdateSystemBarAppearance(
			Context? context,
			AWindow? window,
			bool updateStatusBar,
			bool updateNavigationBar,
			Color? statusBarBackgroundColor = null,
			Color? statusBarForegroundColor = null,
			Color? navigationBarBackgroundColor = null,
			Color? navigationBarForegroundColor = null)
		{
			var activity = context?.GetActivity();
			if (window is null)
			{
				window = activity?.Window;
			}

			window.UpdateSystemBarAppearance(
				activity,
				updateStatusBar,
				updateNavigationBar,
				statusBarBackgroundColor,
				statusBarForegroundColor,
				navigationBarBackgroundColor,
				navigationBarForegroundColor);
		}

		static void UpdateAppBarBackground(AppBarLayout? appBarLayout, Brush? background)
		{
			if (appBarLayout is null)
			{
				return;
			}

			var originalBackground = s_originalAppBarBackgrounds.GetValue(
				appBarLayout,
				static appBar => new OriginalAppBarBackground(appBar.Background?.GetConstantState()));

			if (Brush.IsNullOrEmpty(background))
			{
				appBarLayout.Background = originalBackground.CreateDrawable();
				return;
			}

			if (background is SolidColorBrush { Color: not null } solidColorBrush)
			{
				appBarLayout.SetBackgroundColor(solidColorBrush.Color.ToPlatform());
				return;
			}

			appBarLayout.UpdateBackground(background);
		}

		static Color? GetSolidColor(Brush? background)
		{
			return background is SolidColorBrush { Color: { Alpha: > 0 } color }
				? color
				: null;
		}

		static Color? GetSolidColor(Paint? background)
		{
			return background is SolidPaint { Color: { Alpha: > 0 } color }
				? color
				: null;
		}

		sealed class OriginalAppBarBackground
		{
			readonly Drawable.ConstantState? _backgroundConstantState;

			public OriginalAppBarBackground(Drawable.ConstantState? backgroundConstantState)
			{
				_backgroundConstantState = backgroundConstantState;
			}

			public Drawable? CreateDrawable()
			{
				return _backgroundConstantState?.NewDrawable()?.Mutate();
			}
		}
	}
}
