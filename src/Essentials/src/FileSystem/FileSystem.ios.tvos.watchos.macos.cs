using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using MobileCoreServices;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class FileSystemImplementation : IFileSystem
	{
		public string CacheDirectory
			=> GetDirectory(NSSearchPathDirectory.CachesDirectory);

		public string AppDataDirectory
			=> GetDirectory(NSSearchPathDirectory.LibraryDirectory);

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
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			filename = NormalizePath(filename);

			var root = NSBundle.MainBundle.BundlePath;
#if MACCATALYST || MACOS
			root = Path.Combine(root, "Contents", "Resources");
#endif
			return Path.Combine(root, filename);
		}

		static string NormalizePath(string filename) =>
			filename.Replace('\\', Path.DirectorySeparatorChar);

		static string GetDirectory(NSSearchPathDirectory directory)
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

		internal static string GetContentType(string extension)
		{
			// ios does not like the extensions
			extension = extension?.TrimStart('.');

			// var id = UTType.CreatePreferredIdentifier(UTType.TagClassFilenameExtension, extension, null);
			// var mimeTypes = UTType.CopyAllTags(id, UniformTypeIdentifiers.UTTagClass.MimeType.ToString());
			// return mimeTypes?.Length > 0 ? mimeTypes[0] : null;

			return extension;
		}

		internal void Init(FileBase file)
		{
		}

		internal virtual Task<Stream> OpenReadAsync() =>
			Task.FromResult((Stream)File.OpenRead(FullPath));
	}
}
