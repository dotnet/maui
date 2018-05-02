using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        static string PlatformCacheDirectory
            => ApplicationData.Current.LocalCacheFolder.Path;

        static string PlatformAppDataDirectory
            => ApplicationData.Current.LocalFolder.Path;

        static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            filename = filename.Replace('/', Path.DirectorySeparatorChar);
            return Package.Current.InstalledLocation.OpenStreamForReadAsync(filename);
        }
    }
}
