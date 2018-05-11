using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public partial class SecureStorage
    {
        static Task<string> PlatformGetAsync(string key) =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task PlatformSetAsync(string key, string data) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
