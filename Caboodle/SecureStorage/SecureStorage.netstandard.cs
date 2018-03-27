using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public partial class SecureStorage
    {
        static Task<string> PlatformGetAsync(string key) =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task PlatformSetAsync(string key, string data) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
