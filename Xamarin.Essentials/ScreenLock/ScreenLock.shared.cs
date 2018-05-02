namespace Xamarin.Essentials
{
    public static partial class ScreenLock
    {
        public static bool IsActive => PlatformIsActive;

        public static void RequestActive() => PlatformRequestActive();

        public static void RequestRelease() => PlatformRequestRelease();
    }
}
