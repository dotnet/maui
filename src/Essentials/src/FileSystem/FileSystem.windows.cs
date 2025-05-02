using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Windows.Storage;
using Package = Windows.ApplicationModel.Package;

namespace Microsoft.Maui.Storage
{
	partial class FileSystemImplementation : IFileSystem
	{
		private readonly Lazy<string> _platformCacheDirectory = new(valueFactory: () =>
		{
			if (AppInfoUtils.IsPackagedApp)
			{
				return ApplicationData.Current.LocalCacheFolder.Path;
			}
			else
			{
				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppSpecificPath, "Cache");

				if (!File.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				return path;
			}
		});

		private readonly Lazy<string> _platformAppDataDirectory = new(valueFactory: () =>
		{
			if (AppInfoUtils.IsPackagedApp)
			{
				return ApplicationData.Current.LocalFolder.Path;
			}
			else
			{
				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppSpecificPath, "Data");

				if (!File.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				return path;
			}
		});

		static string CleanPath(string path) =>
			string.Join("_", path.Split(Path.GetInvalidFileNameChars()));

		static string AppSpecificPath =>
			Path.Combine(CleanPath(AppInfoImplementation.PublisherName), CleanPath(AppInfo.PackageName));

		string PlatformCacheDirectory => _platformCacheDirectory.Value;

		string PlatformAppDataDirectory => _platformAppDataDirectory.Value;

		Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			if (AppInfoUtils.IsPackagedApp)
			{
				filename = FileSystemUtils.NormalizePath(filename);

				return Package.Current.InstalledLocation.OpenStreamForReadAsync(filename);
			}
			else
			{
				var file = FileSystemUtils.PlatformGetFullAppPackageFilePath(filename);
				return Task.FromResult((Stream)File.OpenRead(file));
			}
		}

		Task<bool> PlatformAppPackageFileExistsAsync(string filename)
		{
			var file = FileSystemUtils.PlatformGetFullAppPackageFilePath(filename);
			return Task.FromResult(File.Exists(file));
		}
	}

	public partial class FileBase
	{
		internal FileBase(IStorageFile file)
			: this(file?.Path)
		{
			File = file;
			ContentType = file?.ContentType;
		}

		void PlatformInit(FileBase file)
		{
			File = file.File;
		}

		internal IStorageFile File { get; set; }

		// we can't do anything here, but Windows will take care of it
		string PlatformGetContentType(string extension) => null;

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
