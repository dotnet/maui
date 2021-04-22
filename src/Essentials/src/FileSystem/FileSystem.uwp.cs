using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Microsoft.Maui.Essentials
{
	public static partial class FileSystem
	{
		static string PlatformCacheDirectory
			=> ApplicationData.Current.LocalCacheFolder.Path;

		static string PlatformAppDataDirectory
			=> ApplicationData.Current.LocalFolder.Path;

		static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			return Package.Current.InstalledLocation.OpenStreamForReadAsync(NormalizePath(filename));
		}

		internal static string NormalizePath(string path)
			=> path.Replace('/', Path.DirectorySeparatorChar);
	}

	public partial class FileBase
	{
		internal FileBase(IStorageFile file)
			: this(file?.Path)
		{
			File = file;
			ContentType = file?.ContentType;
		}

		internal void PlatformInit(FileBase file)
		{
			File = file.File;
		}

		internal IStorageFile File { get; set; }

		// we can't do anything here, but Windows will take care of it
		internal static string PlatformGetContentType(string extension) => null;

		internal virtual Task<Stream> PlatformOpenReadAsync() =>
			File.OpenStreamForReadAsync();
	}

	public partial class FileResult
	{
		internal FileResult(IStorageFile file)
			: base(file)
		{
		}
	}
}
