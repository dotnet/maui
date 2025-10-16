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
				TryApplyThemeToWindow(window);
			}
		}

		partial void OnPlatformWindowRemoved(Window window)
		{
			window.HandlerChanged -= OnWindowHandlerChanged;
		}

		void OnWindowHandlerChanged(object? sender, EventArgs e)
		{
			if (sender is Window window)
			{
				TryApplyThemeToWindow(window);
			}
		}

		void TryApplyThemeToWindow(Window window)
		{
			if (_currentThemeForWindows is AppTheme theme)
			{
				var forcedElementTheme = GetElementTheme();

				if (IsWindowReady(window))
				{
					ApplyThemeToWindow(window, UserAppTheme == AppTheme.Unspecified, forcedElementTheme);
				}
			}
		}

		bool IsWindowReady(Window window)
		{
			var platformWindow = window?.Handler?.PlatformView as UI.Xaml.Window;
			return platformWindow?.Content is FrameworkElement;
		}

		void ApplyThemeToAllWindows(AppTheme newTheme, bool followSystem)
		{
			var forcedElementTheme = GetElementTheme();

			foreach (var window in Windows)
			{
				if (IsWindowReady(window))
				{
					ApplyThemeToWindow(window, followSystem, forcedElementTheme);
				}
			}
		}

		ElementTheme GetElementTheme()
		{
			if (_currentThemeForWindows is AppTheme theme)
			{
				return theme switch
				{
					AppTheme.Dark => ElementTheme.Dark,
					AppTheme.Light => ElementTheme.Light,
					_ => ElementTheme.Default
				};
			}
			return ElementTheme.Default;
		}

		void ApplyThemeToWindow(Window? window, bool followSystem, ElementTheme forcedElementTheme)
		{
			var platformWindow = window?.Handler?.PlatformView as UI.Xaml.Window;

			if (platformWindow is null)
			{
				System.Diagnostics.Debug.WriteLine("ApplyThemeToWindow: platformWindow is null. Unable to apply theme to the root element.");
				return;
			}

			if (platformWindow.DispatcherQueue is null)
			{
				System.Diagnostics.Debug.WriteLine("ApplyThemeToWindow: platformWindow.DispatcherQueue is null. Unable to apply theme to the root element.");
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

				SetTitleBarButtonColors(platformWindow, isDark);
			});
		}

		void SetTitleBarButtonColors(UI.Xaml.Window platformWindow, bool isDark)
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