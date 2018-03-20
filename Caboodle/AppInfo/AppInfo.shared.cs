using System;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class AppInfo
    {
        public static string PackageName => GetPackageName();

        public static string Name => GetName();

        public static string VersionString => GetVersionString();

        public static Version Version => Utils.ParseVersion(VersionString);

        public static string BuildString => GetBuild();
    }
}
