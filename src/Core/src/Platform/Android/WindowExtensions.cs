using System.Runtime.CompilerServices;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Util;
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
				// HSL luminosity misclassifies perceptually-bright hues (yellow, cyan, lime) as dark
				// because it averages max/min channel values and lands at exactly 0.5 for these colours.
				// Use ITU-R BT.601 perceptual luminance with a gamma-2.0 approximation instead:
				//   L = 0.299R² + 0.587G² + 0.114B²  (squaring approximates linear-light encoding)
				// Threshold 0.25 ≈ 50% perceived brightness.
				return IsPerceptuallyLight(backgroundColor);
			}

			if (foregroundColor?.Alpha > 0)
			{
				return foregroundColor.GetLuminosity() <= 0.5;
			}

			return null;
		}

		// ITU-R BT.601 perceptual luminance with gamma-2.0 approximation.
		// Returns true when the colour is bright enough to need dark (black) system bar icons.
		static bool IsPerceptuallyLight(Color color)
		{
			float r = color.Red * color.Red;
			float g = color.Green * color.Green;
			float b = color.Blue * color.Blue;
			return 0.299f * r + 0.587f * g + 0.114f * b > 0.25f;
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
