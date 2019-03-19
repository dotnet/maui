using System.IO;
using System.Threading.Tasks;
using Foundation;
using MobileCoreServices;

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
            return mimeTypes.Length > 0 ? mimeTypes[0] : null;
        }

        internal void PlatformInit(FileBase file)
        {
        }
    }
}
