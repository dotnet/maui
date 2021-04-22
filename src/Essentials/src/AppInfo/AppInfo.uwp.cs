using System.Globalization;
using Windows.ApplicationModel;
#if NET6_0 || NET5_0
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif
using System;

namespace Microsoft.Maui.Essentials
{
	public static partial class AppInfo
	{
		static string PlatformGetPackageName() => Package.Current.Id.Name;

		static string PlatformGetName() => Package.Current.DisplayName;

		static string PlatformGetVersionString()
		{
			var version = Package.Current.Id.Version;
			return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
		}

		static string PlatformGetBuild() =>
			Package.Current.Id.Version.Build.ToString(CultureInfo.InvariantCulture);

		static void PlatformShowSettingsUI() =>
			Windows.System.Launcher.LaunchUriAsync(new global::System.Uri("ms-settings:appsfeatures-app")).WatchForError();

		static AppTheme PlatformRequestedTheme() =>
			throw new NotImplementedException("WINUI"); //Application.Current.RequestedTheme == ApplicationTheme.Dark ? AppTheme.Dark : AppTheme.Light;
	}
}
