using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;

namespace Microsoft.Maui.Storage
{
	partial class FileSystemImplementation : IFileSystem
	{
		string PlatformCacheDirectory
			=> FileSystemUtils.GetDirectory(NSSearchPathDirectory.CachesDirectory);

		string PlatformAppDataDirectory
			=> FileSystemUtils.GetDirectory(NSSearchPathDirectory.LibraryDirectory);

		Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
		{
			var file = FileSystemUtils.PlatformGetFullAppPackageFilePath(filename);
			return Task.FromResult((Stream)File.OpenRead(file));
		}

		Task<bool> PlatformAppPackageFileExistsAsync(string filename)
		{
			var file = FileSystemUtils.PlatformGetFullAppPackageFilePath(filename);
			return Task.FromResult(File.Exists(file));
		}
	}

	static partial class FileSystemUtils
	{
		public static string PlatformGetFullAppPackageFilePath(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			filename = NormalizePath(filename);

			var root = NSBundle.MainBundle.BundlePath;
#if MACCATALYST || MACOS
			root = Path.Combine(root, "Contents", "Resources");
#endif
			return Path.Combine(root, filename);
		}

		public static string GetDirectory(NSSearchPathDirectory directory)
		{
			var dirs = NSSearchPath.GetDirectories(directory, NSSearchPathDomain.User);
			if (dirs == null || dirs.Length == 0)
			{
				// this should never happen...
				return null;
			}
			return dirs[0];
		}
	}

	public partial class FileBase
	{
		internal FileBase(NSUrl file)
			: this(file?.Path)
		{
			FileName = NSFileManager.DefaultManager.DisplayName(file?.Path);
		}

		string PlatformGetContentType(string extension)
		{
			// ios does not like the extensions
			extension = extension?.TrimStart('.');

			// UTType supports only iOS version greater than 14
			if (OperatingSystem.IsIOSVersionAtLeast(14))
			{
				var uti = UniformTypeIdentifiers.UTType.GetType(extension, UniformTypeIdentifiers.UTTagClass.FilenameExtension, null);
				extension = uti?.PreferredMimeType ?? extension;
			}

			return extension;
		}

		void PlatformInit(FileBase file)
		{
		}

		internal virtual Task<Stream> PlatformOpenReadAsync() =>
			Task.FromResult((Stream)File.OpenRead(FullPath));
	}
}
