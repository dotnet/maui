using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        static string PlatformCacheDirectory
            => GetDirectory(NSSearchPathDirectory.CachesDirectory);

        static string PlatformAppDataDirectory
            => GetDirectory(NSSearchPathDirectory.LibraryDirectory);

        static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            filename = filename.Replace('\\', Path.DirectorySeparatorChar);
            var file = Path.Combine(NSBundle.MainBundle.BundlePath, filename);
            return Task.FromResult((Stream)File.OpenRead(file));
        }

        static string GetDirectory(NSSearchPathDirectory directory)
        {
            var dirs = NSSearchPath.GetDirectories(directory, NSSearchPathDomain.User);
            if (dirs == null || dirs.Length == 0)
            {
                // this should never happen...
                return null;
            }
            return dirs[0];
        }
    }
}
