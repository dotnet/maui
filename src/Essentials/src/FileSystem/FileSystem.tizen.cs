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
			if (string.IsNullOrWhiteSpace(filename))
				throw new ArgumentNullException(nameof(filename));

			filename = filename.Replace('\\', Path.DirectorySeparatorChar);
			Stream fs = File.OpenRead(Path.Combine(Application.Current.DirectoryInfo.Resource, filename));
			return Task.FromResult(fs);
		}
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
