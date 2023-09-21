using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Storage
{

	partial class FileSystemImplementation : IFileSystem
	{

		static string CleanPath(string path) =>
			string.Join("_", path.Split(Path.GetInvalidFileNameChars()));

		static string AppSpecificPath =>
			Path.Combine(CleanPath(AppInfoImplementation.PublisherName), CleanPath(AppInfo.PackageName));

		string PlatformCacheDirectory
			=> Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppSpecificPath, "Cache");

		string PlatformAppDataDirectory
			=> Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppSpecificPath, "Data");

		Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

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

		public static bool AppPackageFileExists(string filename)
		{
			var file = PlatformGetFullAppPackageFilePath(filename);

			return File.Exists(file);
		}

		public static string PlatformGetFullAppPackageFilePath(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			filename = NormalizePath(filename);

			string root;

			root = AppContext.BaseDirectory;

			return Path.Combine(root, filename);
		}

	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FileBase.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FileBase']/Docs" />
	public partial class FileBase
	{

		static string PlatformGetContentType(string extension) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal void Init(FileBase file) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal virtual Task<Stream> PlatformOpenReadAsync()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformInit(FileBase file)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

	}

}