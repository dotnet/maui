using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class Sms
    {
        internal static bool IsComposeSupported => false;

        static Task PlatformComposeAsync(SmsMessage message)
            => throw new NotImplementedInReferenceAssemblyException();
    }
}
