using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		AppTheme? _currentThemeForWindows;
		AppTheme? _lastAppliedTheme;

		partial void OnRequestedThemeChangedPlatform(AppTheme newTheme)
		{
			_currentThemeForWindows = newTheme;

			ApplyThemeToAllWindows(newTheme, UserAppTheme == AppTheme.Unspecified);
		}

		partial void OnPlatformWindowAdded(Window window)
		{
			window.HandlerChanged += OnWindowHandlerChanged;

			if (_currentThemeForWindows is not null && window.Handler is not null)
			{
				TryApplyTheme();
			}
		}

		partial void OnPlatformWindowRemoved(Window window)
		{
			window.HandlerChanged -= OnWindowHandlerChanged;
		}

		void OnWindowHandlerChanged(object? sender, EventArgs e) => TryApplyTheme();

		void TryApplyTheme()
		{
			if (_currentThemeForWindows is AppTheme theme)
			{
				ApplyThemeToAllWindows(theme, UserAppTheme == AppTheme.Unspecified);
			}
		}

		bool AnyWindowReady()
		{
			foreach (var window in Windows)
			{
				var platformWindow = window?.Handler?.PlatformView as UI.Xaml.Window;
				if (platformWindow?.Content is FrameworkElement)
				{
					return true;
				}
			}
			return false;
		}

		void ApplyThemeToAllWindows(AppTheme newTheme, bool followSystem)
		{
			if (!AnyWindowReady())
			{
				return;
			}
			if (_lastAppliedTheme == newTheme && !followSystem)
			{
				return;
			}

			_lastAppliedTheme = newTheme;

			var forcedElementTheme = newTheme switch
			{
				AppTheme.Dark => ElementTheme.Dark,
				AppTheme.Light => ElementTheme.Light,
				_ => ElementTheme.Default
			};

			foreach (var window in Windows)
			{
				ApplyThemeToWindow(window, followSystem, forcedElementTheme);
			}
		}

		void ApplyThemeToWindow(Window? window, bool followSystem, ElementTheme forcedElementTheme)
		{
			var platformWindow = window?.Handler?.PlatformView as UI.Xaml.Window;

			if (platformWindow is null)
			{
				return;
			}

			platformWindow.DispatcherQueue.TryEnqueue(() =>
			{
				if (platformWindow.Content is not FrameworkElement root)
				{
					return;
				}

				// Setting RequestedTheme on the root element automatically applies the theme to all child controls. 
				root.RequestedTheme = followSystem ? ElementTheme.Default : forcedElementTheme;

				var isDark = followSystem
					? (UI.Xaml.Application.Current.RequestedTheme == ApplicationTheme.Dark)
					: (forcedElementTheme == ElementTheme.Dark);

				SetTileBarButtonColors(platformWindow, isDark);
			});
		}

		void SetTileBarButtonColors(UI.Xaml.Window platformWindow, bool isDark)
		{
			// Color references:
			// https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Helpers/TitleBarHelper.cs
			// https://github.com/dotnet/maui/blob/main/src/Core/src/Platform/Windows/MauiWinUIWindow.cs#L218
			if (AppWindowTitleBar.IsCustomizationSupported())
			{
				var titleBar = platformWindow.GetAppWindow()?.TitleBar;
				if (titleBar is not null)
				{
					titleBar.ButtonBackgroundColor = UI.Colors.Transparent;
					titleBar.ButtonInactiveBackgroundColor = UI.Colors.Transparent;
					titleBar.ButtonHoverBackgroundColor = isDark ? TitleBarColors.DarkHoverBackground : TitleBarColors.LightHoverBackground;
					titleBar.ButtonPressedBackgroundColor = isDark ? TitleBarColors.DarkPressedBackground : TitleBarColors.LightPressedBackground;
					titleBar.ButtonHoverForegroundColor = isDark ? TitleBarColors.DarkForeground : TitleBarColors.LightForeground;
					titleBar.ButtonPressedForegroundColor = isDark ? TitleBarColors.DarkForeground : TitleBarColors.LightForeground;
					titleBar.ButtonForegroundColor = isDark ? TitleBarColors.DarkForeground : TitleBarColors.LightForeground;
				}
			}
		}
	}
	static class TitleBarColors
	{
		public static readonly Color LightHoverBackground = UI.ColorHelper.FromArgb(24, 0, 0, 0);
		public static readonly Color DarkHoverBackground = UI.ColorHelper.FromArgb(24, 255, 255, 255);
		public static readonly Color LightPressedBackground = UI.ColorHelper.FromArgb(31, 0, 0, 0);
		public static readonly Color DarkPressedBackground = UI.ColorHelper.FromArgb(31, 255, 255, 255);
		public static readonly Color LightForeground = UI.Colors.Black;
		public static readonly Color DarkForeground = UI.Colors.White;
	}
}