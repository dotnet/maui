namespace Xamarin.Essentials
{
    public static partial class PhoneDialer
    {
        internal static bool IsSupported =>
            throw new System.PlatformNotSupportedException();

        static void PlatformOpen(string number) =>
            throw new System.PlatformNotSupportedException();
    }
}
