using System;
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using CaboodlePlatform = Microsoft.Caboodle.Platform;

namespace Microsoft.Caboodle
{
    public static partial class AppInfo
    {
        static string GetPackageName() => CaboodlePlatform.CurrentContext.PackageName;

        static string GetName()
        {
            var applicationInfo = CaboodlePlatform.CurrentContext.ApplicationInfo;
            var packageManager = CaboodlePlatform.CurrentContext.PackageManager;
            return applicationInfo.LoadLabel(packageManager);
        }

        static string GetVersionString()
        {
            var pm = CaboodlePlatform.CurrentContext.PackageManager;
            var packageName = CaboodlePlatform.CurrentContext.PackageName;
            using (var info = pm.GetPackageInfo(packageName, PackageInfoFlags.MetaData))
            {
                return info.VersionName;
            }
        }

        static string GetBuild()
        {
            var pm = CaboodlePlatform.CurrentContext.PackageManager;
            var packageName = CaboodlePlatform.CurrentContext.PackageName;
            using (var info = pm.GetPackageInfo(packageName, PackageInfoFlags.MetaData))
            {
                return info.VersionCode.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
