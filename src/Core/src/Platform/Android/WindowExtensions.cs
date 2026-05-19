using Android.App;
using Android.Content;
using Android.Content.Res;
using System.Runtime.CompilerServices;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui
{
	public static partial class WindowExtensions
	{
		static readonly ConditionalWeakTable<Window, OriginalSystemBarColors> s_originalSystemBarColors = new();

		internal static void UpdateTitle(this Activity platformWindow, IWindow window)
		{
			if (string.IsNullOrEmpty(window.Title))
				platformWindow.Title = ApplicationModel.AppInfo.Current.Name;
			else
				platformWindow.Title = window.Title;
		}

		internal static DisplayOrientation GetOrientation(this IWindow? window)
		{
			if (window == null)
				return DeviceDisplay.Current.MainDisplayInfo.Orientation;

			return window.Handler?.MauiContext?.GetPlatformWindow()?.Resources?.Configuration?.Orientation switch
			{
				Orientation.Landscape => DisplayOrientation.Landscape,
				Orientation.Portrait => DisplayOrientation.Portrait,
				Orientation.Square => DisplayOrientation.Portrait,
				_ => DisplayOrientation.Unknown
			};
		}

		internal static void UpdateWindowSoftInputModeAdjust(this IWindow platformView, SoftInput inputMode)
		{
			var activity = platformView?.Handler?.PlatformView as Activity ??
							platformView?.Handler?.MauiContext?.GetPlatformWindow();

			activity?
				.Window?
				.SetSoftInputMode(inputMode);
		}

		//TODO : Make it public in NET 11.
		internal static void ConfigureTranslucentSystemBars(this Window? window, Activity activity)
		{
			window.UpdateSystemBarAppearance(activity, updateStatusBar: true, updateNavigationBar: true);
		}

		internal static void UpdateSystemBarAppearance(
			this Window? window,
			Activity? activity,
			bool updateStatusBar,
			bool updateNavigationBar,
			Color? statusBarBackgroundColor = null,
			Color? statusBarForegroundColor = null,
			Color? navigationBarBackgroundColor = null,
			Color? navigationBarForegroundColor = null)
		{
			if (window is null)
			{
				return;
			}

			UpdateSystemBarBackgrounds(
				window,
				updateStatusBar,
				updateNavigationBar,
				statusBarBackgroundColor,
				navigationBarBackgroundColor);

			// Set appropriate system bar appearance for readability using AndroidX compat APIs.
			var windowInsetsController = WindowCompat.GetInsetsController(window, window.DecorView);
			if (windowInsetsController is not null)
			{
				// Automatically adjust icon/text colors based on explicit chrome colors when available,
				// falling back to the app theme when MAUI doesn't know the system bar background.
				var configuration = activity?.Resources?.Configuration;
				var isLightTheme = configuration is null ||
					(configuration.UiMode & UiMode.NightMask) != UiMode.NightYes;

				if (updateStatusBar)
				{
					windowInsetsController.AppearanceLightStatusBars =
						GetLightSystemBarAppearance(statusBarBackgroundColor, statusBarForegroundColor) ?? isLightTheme;
				}

				if (updateNavigationBar)
				{
					windowInsetsController.AppearanceLightNavigationBars =
						GetLightSystemBarAppearance(navigationBarBackgroundColor, navigationBarForegroundColor) ?? isLightTheme;
				}
			}
		}

		static void UpdateSystemBarBackgrounds(
			Window window,
			bool updateStatusBar,
			bool updateNavigationBar,
			Color? statusBarBackgroundColor,
			Color? navigationBarBackgroundColor)
		{
			var originalSystemBarColors = s_originalSystemBarColors.GetValue(
				window,
				static window => new OriginalSystemBarColors(window));

#pragma warning disable CA1422 // System bar color APIs still apply to older Android versions and are harmless on newer versions.
			if (updateStatusBar)
			{
				if (statusBarBackgroundColor?.Alpha > 0)
				{
					window.SetStatusBarColor(statusBarBackgroundColor.ToPlatform());
				}
				else
				{
					originalSystemBarColors.RestoreStatusBarColor(window);
				}
			}

			if (updateNavigationBar)
			{
				if (navigationBarBackgroundColor?.Alpha > 0)
				{
					window.SetNavigationBarColor(navigationBarBackgroundColor.ToPlatform());
				}
				else
				{
					originalSystemBarColors.RestoreNavigationBarColor(window);
				}
			}
#pragma warning restore CA1422
		}

		static bool? GetLightSystemBarAppearance(Color? backgroundColor, Color? foregroundColor)
		{
			if (backgroundColor?.Alpha > 0)
			{
				return backgroundColor.GetLuminosity() > 0.5;
			}

			if (foregroundColor?.Alpha > 0)
			{
				return foregroundColor.GetLuminosity() <= 0.5;
			}

			return null;
		}

		sealed class OriginalSystemBarColors
		{
			readonly int _statusBarColor;
			readonly int _navigationBarColor;

			public OriginalSystemBarColors(Window window)
			{
#pragma warning disable CA1422
				_statusBarColor = window.StatusBarColor;
				_navigationBarColor = window.NavigationBarColor;
#pragma warning restore CA1422
			}

			public void RestoreStatusBarColor(Window window)
			{
				window.SetStatusBarColor(new AColor(_statusBarColor));
			}

			public void RestoreNavigationBarColor(Window window)
			{
				window.SetNavigationBarColor(new AColor(_navigationBarColor));
			}
		}
	}
}
