using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        static string PlatformCacheDirectory
            => throw new NotImplementedInReferenceAssemblyException();

        static string PlatformAppDataDirectory
            => throw new NotImplementedInReferenceAssemblyException();

        static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
             => throw new NotImplementedInReferenceAssemblyException();
    }
}
