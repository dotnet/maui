using System;
using System.IO;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class FileSystemImplementation : IFileSystem
	{
		string CacheDirectory
			=> Application.Current.DirectoryInfo.Cache;

		string AppDataDirectory
			=> Application.Current.DirectoryInfo.Data;

		public Task<Stream> OpenAppPackageFileAsync(string filename)
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
		static string GetContentType(string extension)
		{
			extension = extension.TrimStart('.');
			return Tizen.Content.MimeType.MimeUtil.GetMimeType(extension);
		}

		internal void Init(FileBase file)
		{
		}

		internal virtual async Task<Stream> OpenReadAsync()
		{
			await Permissions.RequestAsync<Permissions.StorageRead>();

			var stream = File.Open(FullPath, FileMode.Open, FileAccess.Read);
			return Task.FromResult<Stream>(stream).Result;
		}
	}
}
