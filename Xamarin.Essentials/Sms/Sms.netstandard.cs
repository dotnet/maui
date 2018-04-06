using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Sms
    {
        internal static bool IsComposeSupported => false;

        static Task PlatformComposeAsync(SmsMessage message)
            => throw new NotImplementedInReferenceAssemblyException();
    }
}
