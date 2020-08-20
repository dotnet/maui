using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Provider;
using Android.Webkit;

using Bitmap = global::Android.Graphics.Bitmap;

namespace Xamarin.Essentials
{
    public partial class FileSystem
    {
        static string PlatformCacheDirectory
            => Platform.AppContext.CacheDir.AbsolutePath;

        static string PlatformAppDataDirectory
            => Platform.AppContext.FilesDir.AbsolutePath;

        static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            filename = filename.Replace('\\', Path.DirectorySeparatorChar);
            try
            {
                return Task.FromResult(Platform.AppContext.Assets.Open(filename));
            }
            catch (Java.IO.FileNotFoundException ex)
            {
                throw new FileNotFoundException(ex.Message, filename, ex);
            }
        }
    }

    public partial class FileBase
    {
        internal FileBase(Java.IO.File file)
            : this(file?.Path)
        {
        }

        internal FileBase(global::Android.Net.Uri contentUri)
           : this(GetFullPath(contentUri))
        {
            this.contentUri = contentUri;
            FileName = GetFileName(contentUri);
        }

        readonly global::Android.Net.Uri contentUri;

        internal static string PlatformGetContentType(string extension) =>
            MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension.TrimStart('.'));

        static string GetFullPath(global::Android.Net.Uri contentUri)
        {
            // if this is a file, use that
            if (contentUri.Scheme == "file")
                return contentUri.Path;

            // ask the content provider for the data column, which may contain the actual file path
#pragma warning disable CS0618 // Type or member is obsolete
            var path = QueryContentResolverColumn(contentUri, MediaStore.Files.FileColumns.Data);
#pragma warning restore CS0618 // Type or member is obsolete

            if (!string.IsNullOrEmpty(path) && Path.IsPathRooted(path))
                return path;

            // fallback: use content URI
            return contentUri.ToString();
        }

        static string GetFileName(global::Android.Net.Uri contentUri)
        {
            // resolve file name by querying content provider for display name
            var filename = QueryContentResolverColumn(contentUri, MediaStore.MediaColumns.DisplayName);

            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = Path.GetFileName(WebUtility.UrlDecode(contentUri.ToString()));
            }

            if (!Path.HasExtension(filename))
                filename = filename.TrimEnd('.') + '.' + GetFileExtensionFromUri(contentUri);

            return filename;
        }

        static string QueryContentResolverColumn(global::Android.Net.Uri contentUri, string columnName)
        {
            string text = null;

            var projection = new[] { columnName };
            using var cursor = Application.Context.ContentResolver.Query(contentUri, projection, null, null, null);
            if (cursor?.MoveToFirst() == true)
            {
                var columnIndex = cursor.GetColumnIndex(columnName);
                if (columnIndex != -1)
                    text = cursor.GetString(columnIndex);
            }

            return text;
        }

        static string GetFileExtensionFromUri(global::Android.Net.Uri uri)
        {
            var mimeType = Application.Context.ContentResolver.GetType(uri);
            return mimeType != null ? global::Android.Webkit.MimeTypeMap.Singleton.GetExtensionFromMimeType(mimeType) : string.Empty;
        }

        internal void PlatformInit(FileBase file)
        {
        }

        internal virtual Task<Stream> PlatformOpenReadAsync()
        {
            if (contentUri?.Scheme == "content")
            {
                var content = Application.Context.ContentResolver.OpenInputStream(contentUri);
                return Task.FromResult(content);
            }

            var stream = File.OpenRead(FullPath);
            return Task.FromResult<Stream>(stream);
        }
    }

    public partial class FileResult
    {
        internal FileResult(global::Android.Net.Uri contentUri)
            : base(contentUri)
        {
        }
    }
}
