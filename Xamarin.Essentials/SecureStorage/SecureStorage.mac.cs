using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public partial class SecureStorage
    {
        static Task<string> PlatformGetAsync(string key) =>
            throw new System.PlatformNotSupportedException();

        static Task PlatformSetAsync(string key, string data) =>
            throw new System.PlatformNotSupportedException();

        static bool PlatformRemove(string key) =>
            throw new System.PlatformNotSupportedException();

        static void PlatformRemoveAll() =>
            throw new System.PlatformNotSupportedException();
    }
}
