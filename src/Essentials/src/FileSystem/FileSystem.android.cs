using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Webkit;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Storage
{
	partial class FileSystemImplementation : IFileSystem
	{
		string PlatformCacheDirectory
			=> Application.Context.CacheDir.AbsolutePath;

		string PlatformAppDataDirectory
			=> Application.Context.FilesDir.AbsolutePath;

		Task<Stream> PlatformOpenAppPackageFileAsync(string filename) =>
			Task.FromResult(PlatformOpenAppPackageFile(filename));

		Task<bool> PlatformAppPackageFileExistsAsync(string filename)
		{
			try
			{
				using var stream = PlatformOpenAppPackageFile(filename);
				return Task.FromResult(true);
			}
			catch (FileNotFoundException)
			{
				return Task.FromResult(false);
			}
		}

		Stream PlatformOpenAppPackageFile(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			filename = FileSystemUtils.NormalizePath(filename);

			try
			{
				return Application.Context.Assets.Open(filename);
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

		internal string PlatformGetContentType(string extension) =>
			MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension.TrimStart('.'));

		void PlatformInit(FileBase file)
		{
		}

		internal virtual Task<Stream> PlatformOpenReadAsync()
		{
			var stream = File.OpenRead(FullPath);
			return Task.FromResult<Stream>(stream);
		}
	}
	
	public partial class MediaFileResult
	{
		readonly Android.Net.Uri _uri;
		readonly string _tempFilePath;

		internal MediaFileResult(string fileName, Android.Net.Uri uri, string tempFilePath = null)
		{
			_tempFilePath = tempFilePath;
			_uri = uri;
			NameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			Extension = Path.GetExtension(fileName).TrimStart('.');
			ContentType = PlatformGetContentType(Extension);
			Type = GetFileType(ContentType);
			FileName = GetFileName(NameWithoutExtension, Extension);
		}

		internal override Task<Stream> PlatformOpenReadAsync()
			=> Task.FromResult(Platform.AppContext.ContentResolver!.OpenInputStream(_uri));

		void PlatformDispose()
		{
			if(!string.IsNullOrWhiteSpace(_tempFilePath) && File.Exists(_tempFilePath))
				File.Delete(_tempFilePath);

			_uri?.Dispose();
		}
	}
}
