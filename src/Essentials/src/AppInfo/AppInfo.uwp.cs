using System.Globalization;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

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
            Windows.System.Launcher.LaunchUriAsync(new System.Uri("ms-settings:appsfeatures-app")).WatchForError();

        static AppTheme PlatformRequestedTheme() =>
            Application.Current.RequestedTheme == ApplicationTheme.Dark ? AppTheme.Dark : AppTheme.Light;
    }
}
