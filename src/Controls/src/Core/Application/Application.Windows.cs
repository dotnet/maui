using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		AppTheme? _currentThemeForWindows;

		partial void OnRequestedThemeChangedPlatform(AppTheme newTheme)
		{
			_currentThemeForWindows = newTheme;

			if (!AnyWindowReady())
			{
				return;
			}

			ApplyThemeToAllWindows(newTheme, UserAppTheme == AppTheme.Unspecified);
		}

		partial void OnWindowAddedPlatform(Window window)
		{
			window.HandlerChanged += OnWindowHandlerChanged;

			if (_currentThemeForWindows is not null && window.Handler is not null)
			{
				TryApplyTheme();
			}
		}

		void OnWindowHandlerChanged(object? sender, EventArgs e) => TryApplyTheme();

		void TryApplyTheme()
		{
			if (!AnyWindowReady())
			{
				return;
			}

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

				root.RequestedTheme = followSystem ? ElementTheme.Default : forcedElementTheme;

				if (AppWindowTitleBar.IsCustomizationSupported())
				{
					var titleBar = platformWindow.GetAppWindow()?.TitleBar;
					if (titleBar is not null)
					{
						var isDark = followSystem
							? (UI.Xaml.Application.Current.RequestedTheme == ApplicationTheme.Dark)
							: (forcedElementTheme == ElementTheme.Dark);

						titleBar.ButtonBackgroundColor = UI.Colors.Transparent;
						titleBar.ButtonInactiveBackgroundColor = UI.Colors.Transparent;
						titleBar.ButtonForegroundColor = isDark ? UI.Colors.White : UI.Colors.Black;
						titleBar.PreferredTheme = isDark ? TitleBarTheme.Dark : TitleBarTheme.Light;
					}
				}
			});
		}
	}
}