namespace Xamarin.Essentials
{
    public static partial class PhoneDialer
    {
        internal static bool IsSupported => false;

        public static void Open(string number) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
