using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.UI.ViewManagement;

namespace Microsoft.Caboodle
{
    public static partial class AppInfo
    {
        static string GetPackageName() => Package.Current.Id.Name;

        static string GetName() => Package.Current.DisplayName;

        static string GetVersionString()
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        static string GetBuild()
            => Package.Current.Id.Version.Build.ToString(CultureInfo.InvariantCulture);
    }
}
