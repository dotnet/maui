namespace Xamarin.Essentials
{
    public static partial class ScreenLock
    {
        static bool PlatformIsActive
            => throw new NotImplementedInReferenceAssemblyException();

        static void PlatformRequestActive()
            => throw new NotImplementedInReferenceAssemblyException();

        static void PlatformRequestRelease()
            => throw new NotImplementedInReferenceAssemblyException();
    }
}
