using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Email
    {
        internal static bool IsComposeSupported => false;

        static Task PlatformComposeAsync(EmailMessage message) =>
                throw new NotImplementedInReferenceAssemblyException();
    }
}
