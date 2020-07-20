using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        public static string CacheDirectory
            => PlatformCacheDirectory;

        public static string AppDataDirectory
            => PlatformAppDataDirectory;

        public static Task<Stream> OpenAppPackageFileAsync(string filename)
            => PlatformOpenAppPackageFileAsync(filename);
    }

    public abstract partial class FileBase
    {
        internal const string DefaultContentType = "application/octet-stream";

        string contentType;

        internal FileBase(string fullPath)
        {
            if (fullPath == null)
                throw new ArgumentNullException(nameof(fullPath));
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("The attachment file path cannot be an empty string.", nameof(fullPath));
            if (string.IsNullOrWhiteSpace(Path.GetFileName(fullPath)))
                throw new ArgumentException("The attachment file path must be a file path.", nameof(fullPath));

            FullPath = fullPath;
        }

        public FileBase(FileBase file)
        {
            FullPath = file.FullPath;
            ContentType = file.ContentType;
            PlatformInit(file);
        }

        internal FileBase(string fullPath, string contentType)
            : this(fullPath)
        {
            FullPath = fullPath;
            ContentType = contentType;
        }

        public string FullPath { get; }

        public string ContentType
        {
            get => GetContentType();
            set => contentType = value;
        }

        internal string GetContentType()
        {
            // try the provided type
            if (!string.IsNullOrWhiteSpace(contentType))
                return contentType;

            // try get from the file extension
            var ext = Path.GetExtension(FullPath);
            if (!string.IsNullOrWhiteSpace(ext))
            {
                var content = PlatformGetContentType(ext);
                if (!string.IsNullOrWhiteSpace(content))
                    return content;
            }
            return "application/octet-stream";
        }

        string attachmentName;

        internal string AttachmentName
        {
            get => GetAttachmentName();
            set => attachmentName = value;
        }

        internal string GetAttachmentName()
        {
            // try the provided file name
            if (!string.IsNullOrWhiteSpace(attachmentName))
                return attachmentName;

            // try get from the path
            if (!string.IsNullOrWhiteSpace(FullPath))
                return Path.GetFileName(FullPath);

            // this should never happen as the path is validated in the constructor
            throw new InvalidOperationException($"Unable to determine the attachment file name from '{FullPath}'.");
        }
    }

    public class ReadOnlyFile : FileBase
    {
        public ReadOnlyFile(string fullPath)
            : base(fullPath)
        {
        }

        public ReadOnlyFile(string fullPath, string contentType)
            : base(fullPath, contentType)
        {
        }

        public ReadOnlyFile(FileBase file)
            : base(file)
        {
        }
    }
}
