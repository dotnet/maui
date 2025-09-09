using Microsoft.Maui.ApplicationModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		
		// For Windows, we need to manually propagate theme changes to each window root view.
		// This is because the theme resources are not automatically updated when the user app theme changes.
		// Due to this, the control styles do not get updated to reflect the new theme.
		partial void OnRequestedThemeChangedPlatform(AppTheme newTheme)
		{
			bool followSystemTheme = UserAppTheme == AppTheme.Unspecified;

			ElementTheme forcedTheme = newTheme switch
			{
				AppTheme.Dark => ElementTheme.Dark,
				AppTheme.Light => ElementTheme.Light,
				_ => ElementTheme.Default
			};

			foreach (var window in Windows)
			{
				var platformWindow = window?.Handler?.PlatformView as UI.Xaml.Window;

				if (platformWindow is null)
				{
					continue;
				}

				if (platformWindow.Content is FrameworkElement root)
				{
					root.RequestedTheme = followSystemTheme ? ElementTheme.Default : forcedTheme;
					root.RefreshThemeResources();
				}

				if (AppWindowTitleBar.IsCustomizationSupported())
				{
					var titleBar = platformWindow.GetAppWindow()?.TitleBar;
					if (titleBar is not null)
					{
						var isDark = followSystemTheme
						? (UI.Xaml.Application.Current.RequestedTheme == ApplicationTheme.Dark)
						: (forcedTheme == ElementTheme.Dark);

						titleBar.ButtonBackgroundColor = UI.Colors.Transparent;
						titleBar.ButtonInactiveBackgroundColor = UI.Colors.Transparent;
						titleBar.ButtonForegroundColor = isDark ? UI.Colors.White : UI.Colors.Black;

						titleBar.PreferredTheme = isDark ? TitleBarTheme.Dark : TitleBarTheme.Light;

					}
				}
			}
		}
	}
}