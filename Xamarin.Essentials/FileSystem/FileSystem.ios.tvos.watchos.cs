using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using MobileCoreServices;

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

    public partial class FileBase
    {
        internal FileBase(NSUrl file)
            : this(NSFileManager.DefaultManager.DisplayName(file?.Path))
        {
        }

        internal static string PlatformGetContentType(string extension)
        {
            // ios does not like the extensions
            extension = extension?.TrimStart('.');

            var id = UTType.CreatePreferredIdentifier(UTType.TagClassFilenameExtension, extension, null);
            var mimeTypes = UTType.CopyAllTags(id, UTType.TagClassMIMEType);
            if (mimeTypes != null && mimeTypes.Length > 0)
            {
                return mimeTypes[0];
            }
            else
            {
                return null;
            }
        }

        internal void PlatformInit(FileBase file)
        {
        }
    }
}
