using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Win32;
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

			var fileContentType = file?.ContentType;
			if (string.IsNullOrWhiteSpace(fileContentType) ||
				string.Equals(fileContentType, FileMimeTypes.OctetStream, StringComparison.OrdinalIgnoreCase))
			{
				var registryContentType = PlatformGetContentType(Path.GetExtension(file?.Path ?? string.Empty));
				ContentType = !string.IsNullOrWhiteSpace(registryContentType)
					? registryContentType
					: fileContentType;
			}
			else
			{
				ContentType = fileContentType;
			}
		}

		void PlatformInit(FileBase file)
		{
			File = file.File;
		}

		internal IStorageFile File { get; set; }

		static string PlatformGetContentType(string extension)
		{
			if (string.IsNullOrWhiteSpace(extension))
				return null;

			extension = extension.Trim();

			if (!extension.StartsWith("."))
				extension = "." + extension;

			try
			{
#pragma warning disable CA1416 // Validate platform compatibility
                using var key = Registry.ClassesRoot.OpenSubKey(extension);
                if (key?.GetValue("Content Type") is string contentType &&
					!string.IsNullOrWhiteSpace(contentType))
				{
					return contentType;
				}
#pragma warning restore CA1416 // Validate platform compatibility
            }
			catch
			{
				// Registry access can fail in sandboxed environments; fall through to default content type.
			}

			return null;
		}

		internal async virtual Task<Stream> PlatformOpenReadAsync()
		{
			if (File is null && FullPath is not null)
			{
				File = await StorageFile.GetFileFromPathAsync(FullPath);
			}

			return await File.OpenStreamForReadAsync();
		}
	}

	public partial class FileResult
	{
		internal FileResult(IStorageFile file)
			: base(file)
		{
		}
	}
}
