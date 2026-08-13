using System.Runtime.CompilerServices;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using AColor = Android.Graphics.Color;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

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
			if (!RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
			{
				ConfigureLegacyTranslucentSystemBars(window, activity);
				return;
			}

			window.UpdateSystemBarAppearance(activity, updateStatusBar: true, updateNavigationBar: true);
		}

		static void ConfigureLegacyTranslucentSystemBars(Window? window, Activity activity)
		{
			if (window is null)
			{
				return;
			}

			var windowInsetsController = WindowCompat.GetInsetsController(window, window.DecorView);
			if (windowInsetsController is not null)
			{
				var configuration = activity.Resources?.Configuration;
				var isLightTheme = configuration is null ||
					(configuration.UiMode & UiMode.NightMask) != UiMode.NightYes;

				// Resolve the actual status bar background color from the current theme and
				// choose icon/text appearance based on its luminance. If the theme color cannot
				// be resolved, preserve the previous theme-based behavior.
				if (TryGetThemeColor(activity, global::Android.Resource.Attribute.ColorPrimary, out var statusBarColor))
					windowInsetsController.AppearanceLightStatusBars = IsLightColor(statusBarColor);
				else
					windowInsetsController.AppearanceLightStatusBars = isLightTheme;

				windowInsetsController.AppearanceLightNavigationBars = isLightTheme;
			}
		}

		static bool TryGetThemeColor(Activity activity, int attribute, out AColor color)
		{
			color = default;

			if (activity.Theme is null)
				return false;

			using var ta = activity.Theme.ObtainStyledAttributes([attribute]);

			if (!ta.HasValue(0))
				return false;

			color = new AColor(ta.GetColor(0, 0));
			return true;
		}

		static bool IsLightColor(AColor color) =>
			AndroidX.Core.Graphics.ColorUtils.CalculateLuminance(color.ToArgb()) > 0.5;

		internal static void UpdateSystemBarAppearance(
			this Window? window,
			Activity? activity,
			bool updateStatusBar,
			bool updateNavigationBar,
			Color? statusBarBackgroundColor = null,
			Color? navigationBarBackgroundColor = null)
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

		}

		internal static void UpdateStatusBarTheme(this Window? window, Activity activity, StatusBarTheme statusBarTheme)
		{
			if (window is null)
			{
				return;
			}

			var windowInsetsController = WindowCompat.GetInsetsController(window, window.DecorView);
			if (windowInsetsController is not null)
			{
				windowInsetsController.AppearanceLightStatusBars = statusBarTheme switch
				{
					StatusBarTheme.Light => true,
					StatusBarTheme.Dark => false,
					_ => IsLightTheme(activity),
				};
			}
		}

		static bool IsLightTheme(Activity? activity)
		{
			var configuration = activity?.Resources?.Configuration;
			return configuration is null ||
				(configuration.UiMode & UiMode.NightMask) != UiMode.NightYes;
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
				var restoreColor = new AColor(_navigationBarColor);

				// Only substitute the transparent edge-to-edge default navigation bar color.
				// Preserve any explicit captured native value (including opaque black 0xFF000000)
				// so temporary MAUI overrides can be restored exactly.
				if (_navigationBarColor == 0 && window.Context?.Theme is { } theme)
				{
					var typedValue = new TypedValue();
					if (theme.ResolveAttribute(global::Android.Resource.Attribute.ColorBackground, typedValue, true)
						&& typedValue.Data != 0)
					{
						restoreColor = new AColor(typedValue.Data);
					}
				}

				window.SetNavigationBarColor(restoreColor);
			}
		}
	}
}