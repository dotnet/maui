using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class FileSystemImplementation : IFileSystem
	{
		string CacheDirectory
			=> ApplicationData.Current.LocalCacheFolder.Path;

		string AppDataDirectory
			=> ApplicationData.Current.LocalFolder.Path;

		public Task<Stream> OpenAppPackageFileAsync(string filename)
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

		internal void Init(FileBase file)
		{
			File = file.File;
		}

		internal IStorageFile File { get; set; }

		// we can't do anything here, but Windows will take care of it
		internal static string GetContentType(string extension) => null;

		internal virtual Task<Stream> OpenReadAsync() =>
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
