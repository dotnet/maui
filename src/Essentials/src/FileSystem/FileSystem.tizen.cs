using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Tizen.Applications;

namespace Microsoft.Maui.Storage
{
	partial class FileSystemImplementation : IFileSystem
	{
		string PlatformCacheDirectory
			=> Application.Current.DirectoryInfo.Cache;

		string PlatformAppDataDirectory
			=> Application.Current.DirectoryInfo.Data;

		Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
		{
			var file = PlatformGetFullAppPackageFilePath(filename);
			return Task.FromResult((Stream)File.OpenRead(file));
		}

		Task<bool> PlatformAppPackageFileExistsAsync(string filename)
		{
			var file = PlatformGetFullAppPackageFilePath(filename);
			return Task.FromResult(File.Exists(file));
		}

		static string PlatformGetFullAppPackageFilePath(string filename)
		{
			if (string.IsNullOrWhiteSpace(filename))
				throw new ArgumentNullException(nameof(filename));

			return FileSystemUtils.GetSecurePath(Application.Current.DirectoryInfo.Resource, filename);
		}
	}

	public partial class FileBase
	{
		string PlatformGetContentType(string extension)
		{
			extension = extension.TrimStart('.');
			return Tizen.Content.MimeType.MimeUtil.GetMimeType(extension);
		}

		void PlatformInit(FileBase file)
		{
		}

		internal virtual async Task<Stream> PlatformOpenReadAsync()
		{
			await Permissions.RequestAsync<Permissions.StorageRead>();

			var stream = File.Open(FullPath, FileMode.Open, FileAccess.Read);
			return Task.FromResult<Stream>(stream).Result;
		}
	}
}
