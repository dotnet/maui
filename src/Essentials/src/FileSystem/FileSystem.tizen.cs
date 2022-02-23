using System;
using System.IO;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Microsoft.Maui.Essentials
{
	public static partial class FileSystem
	{
		static string PlatformCacheDirectory
			=> Application.Current.DirectoryInfo.Cache;

		static string PlatformAppDataDirectory
			=> Application.Current.DirectoryInfo.Data;

		static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
		{
			var file = PlatformGetFullAppPackageFilePath(filename);
			return Task.FromResult((Stream)File.OpenRead(file));
		}

		static Task<bool> PlatformAppPackageFileExistsAsync(string filename)
		{
			var file = PlatformGetFullAppPackageFilePath(filename);
			return Task.FromResult(File.Exists(file));
		}

		static string PlatformGetFullAppPackageFilePath(string filename)
		{
			if (string.IsNullOrWhiteSpace(filename))
				throw new ArgumentNullException(nameof(filename));

			filename = NormalizePath(filename);

			return Path.Combine(Application.Current.DirectoryInfo.Resource, filename);
		}

		static string NormalizePath(string filename) =>
			filename.Replace('\\', Path.DirectorySeparatorChar);
	}

	public partial class FileBase
	{
		static string PlatformGetContentType(string extension)
		{
			extension = extension.TrimStart('.');
			return Tizen.Content.MimeType.MimeUtil.GetMimeType(extension);
		}

		internal void PlatformInit(FileBase file)
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
