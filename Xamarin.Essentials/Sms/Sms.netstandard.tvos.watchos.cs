using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Sms
    {
        internal static bool IsComposeSupported
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task PlatformComposeAsync(SmsMessage message)
            => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
