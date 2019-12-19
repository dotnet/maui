using System.Globalization;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class AppInfo
    {
        static string PlatformGetPackageName()
            => Application.Current.ApplicationInfo.PackageId;

        static string PlatformGetName()
            => Application.Current.ApplicationInfo.Label;

        static string PlatformGetVersionString()
            => Platform.CurrentPackage.Version;

        static string PlatformGetBuild()
            => Version.Build.ToString(CultureInfo.InvariantCulture);

        static void PlatformShowSettingsUI()
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();
            AppControl.SendLaunchRequest(new AppControl() { Operation = AppControlOperations.Setting });
        }

        static AppTheme PlatformRequestedTheme()
        {
            return AppTheme.Unspecified;
        }
    }
}
