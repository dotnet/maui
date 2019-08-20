namespace Xamarin.Essentials
{
    public static partial class Preferences
    {
        static bool PlatformContainsKey(string key, string sharedName) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static void PlatformRemove(string key, string sharedName) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static void PlatformClear(string sharedName) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static void PlatformSet<T>(string key, T value, string sharedName) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static T PlatformGet<T>(string key, T defaultValue, string sharedName) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
