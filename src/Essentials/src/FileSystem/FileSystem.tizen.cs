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
			if (string.IsNullOrWhiteSpace(filename))
				throw new ArgumentNullException(nameof(filename));

			filename = filename.Replace('\\', Path.DirectorySeparatorChar);
			Stream fs = File.OpenRead(Path.Combine(Application.Current.DirectoryInfo.Resource, filename));
			return Task.FromResult(fs);
		}
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
