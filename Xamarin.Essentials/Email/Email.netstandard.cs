using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Email
    {
        internal static bool IsComposeSupported =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task PlatformComposeAsync(EmailMessage message) =>
            throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class EmailAttachment
    {
        string PlatformGetContentType(string extension) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
