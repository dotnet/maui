using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        static string PlatformCacheDirectory
            => throw new System.PlatformNotSupportedException();

        static string PlatformAppDataDirectory
            => throw new System.PlatformNotSupportedException();

        static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
             => throw new System.PlatformNotSupportedException();
    }
}
