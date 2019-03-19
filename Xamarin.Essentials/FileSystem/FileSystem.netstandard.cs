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

    public partial class FileBase
    {
        static string PlatformGetContentType(string extension) =>
            throw new NotImplementedInReferenceAssemblyException();

        internal void PlatformInit(FileBase file) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
