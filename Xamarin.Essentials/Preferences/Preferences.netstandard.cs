namespace Xamarin.Essentials
{
    public static partial class Preferences
    {
        static bool PlatformContainsKey(string key, string sharedName) =>
            throw new NotImplementedInReferenceAssemblyException();

        static void PlatformRemove(string key, string sharedName) =>
            throw new NotImplementedInReferenceAssemblyException();

        static void PlatformClear(string sharedName) =>
            throw new NotImplementedInReferenceAssemblyException();

        static void PlatformSet<T>(string key, T value, string sharedName) =>
            throw new NotImplementedInReferenceAssemblyException();

        static T PlatformGet<T>(string key, T defaultValue, string sharedName) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
