namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static bool PlatformKeepScreenOn
        {
            get => throw new System.PlatformNotSupportedException();
            set => throw new System.PlatformNotSupportedException();
        }

        static DisplayInfo GetMainDisplayInfo() => throw new System.PlatformNotSupportedException();

        static void StartScreenMetricsListeners() => throw new System.PlatformNotSupportedException();

        static void StopScreenMetricsListeners() => throw new System.PlatformNotSupportedException();
    }
}
