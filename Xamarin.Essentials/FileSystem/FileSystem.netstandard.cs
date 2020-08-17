using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        static string PlatformCacheDirectory
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        static string PlatformAppDataDirectory
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
             => throw ExceptionUtils.NotSupportedOrImplementedException;
    }

    public partial class FileBase
    {
        static string PlatformGetContentType(string extension) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        internal void PlatformInit(FileBase file) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        internal virtual Task<Stream> PlatformOpenReadAsync()
            => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
