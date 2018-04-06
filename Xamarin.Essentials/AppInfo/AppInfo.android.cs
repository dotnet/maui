using System.Globalization;
using Android.Content.PM;

namespace Xamarin.Essentials
{
    public static partial class AppInfo
    {
        static string GetPackageName() => Platform.CurrentContext.PackageName;

        static string GetName()
        {
            var applicationInfo = Platform.CurrentContext.ApplicationInfo;
            var packageManager = Platform.CurrentContext.PackageManager;
            return applicationInfo.LoadLabel(packageManager);
        }

        static string GetVersionString()
        {
            var pm = Platform.CurrentContext.PackageManager;
            var packageName = Platform.CurrentContext.PackageName;
            using (var info = pm.GetPackageInfo(packageName, PackageInfoFlags.MetaData))
            {
                return info.VersionName;
            }
        }

        static string GetBuild()
        {
            var pm = Platform.CurrentContext.PackageManager;
            var packageName = Platform.CurrentContext.PackageName;
            using (var info = pm.GetPackageInfo(packageName, PackageInfoFlags.MetaData))
            {
                return info.VersionCode.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
