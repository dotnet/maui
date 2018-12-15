namespace Xamarin.Essentials
{
    public static partial class AppInfo
    {
        static string PlatformGetPackageName() => throw new System.PlatformNotSupportedException();

        static string PlatformGetName() => throw new System.PlatformNotSupportedException();

        static string PlatformGetVersionString() => throw new System.PlatformNotSupportedException();

        static string PlatformGetBuild() => throw new System.PlatformNotSupportedException();

        static void PlatformShowSettingsUI() => throw new System.PlatformNotSupportedException();
    }
}
