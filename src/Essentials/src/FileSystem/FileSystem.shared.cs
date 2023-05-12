#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Storage
{
	/// <summary>
	/// Provides an easy way to access the locations for device folders.
	/// </summary>
	public interface IFileSystem
	{
		/// <summary>
		/// Gets the location where temporary data can be stored.
		/// </summary>
		/// <remarks>This location usually is not visible to the user, is not backed up, and may be cleared by the operating system at any time.</remarks>
		string CacheDirectory { get; }

		/// <summary>
		/// Gets the location where app data can be stored.
		/// </summary>
		/// <remarks>This location usually is not visible to the user, and is backed up.</remarks>
		string AppDataDirectory { get; }

		/// <summary>
		/// Opens a stream to a file contained within the app package.
		/// </summary>
		/// <param name="filename">The name of the file (excluding the path) to load from the app package.</param>
		/// <returns>A <see cref="Stream"/> containing the (read-only) file data.</returns>
		Task<Stream> OpenAppPackageFileAsync(string filename);

		/// <summary>
		/// Determines whether or not a file exists in the app package.
		/// </summary>
		/// <param name="filename">The name of the file (excluding the path) to load from the app package.</param>
		/// <returns><see langword="true"/> when the specified file exists in the app package, otherwise <see langword="false"/>.</returns>
		Task<bool> AppPackageFileExistsAsync(string filename);
	}

	/// <summary>
	/// Provides an easy way to access the locations for device folders.
	/// </summary>
	public static class FileSystem
	{
		/// <summary>
		/// Gets the location where temporary data can be stored.
		/// </summary>
		/// <remarks>This location usually is not visible to the user, is not backed up, and may be cleared by the operating system at any time.</remarks>
		public static string CacheDirectory
			=> Current.CacheDirectory;

		/// <summary>
		/// Gets the location where app data can be stored.
		/// </summary>
		/// <remarks>This location usually is not visible to the user, and is backed up.</remarks>
		public static string AppDataDirectory
			=> Current.AppDataDirectory;

		/// <summary>
		/// Opens a stream to a file contained within the app package.
		/// </summary>
		/// <param name="filename">The name of the file (excluding the path) to load from the app package.</param>
		/// <returns>A <see cref="Stream"/> containing the (read-only) file data.</returns>
		public static Task<Stream> OpenAppPackageFileAsync(string filename)
			=> Current.OpenAppPackageFileAsync(filename);

		/// <summary>
		/// Determines whether or not a file exists in the app package.
		/// </summary>
		/// <param name="filename">The path of the file (relative to the app package) to check the existence of.</param>
		/// <returns><see langword="true"/> when the specified file exists in the app package, otherwise <see langword="false"/>.</returns>
		public static Task<bool> AppPackageFileExistsAsync(string filename)
			=> Current.AppPackageFileExistsAsync(filename);

		static IFileSystem? currentImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IFileSystem Current =>
			currentImplementation ??= new FileSystemImplementation();

		internal static void SetCurrent(IFileSystem? implementation) =>
			currentImplementation = implementation;
	}

	/// <summary>
	/// Concrete implementation of the <see cref="IFileSystem"/> APIs.
	/// </summary>
	public partial class FileSystemImplementation
	{
		/// <inheritdoc cref="IFileSystem.CacheDirectory"/>
		public string CacheDirectory
			=> PlatformCacheDirectory;

		/// <inheritdoc cref="IFileSystem.AppDataDirectory"/>
		public string AppDataDirectory
			=> PlatformAppDataDirectory;

		/// <inheritdoc cref="IFileSystem.OpenAppPackageFileAsync(string)"/>
		public Task<Stream> OpenAppPackageFileAsync(string filename)
			=> PlatformOpenAppPackageFileAsync(filename);

		/// <inheritdoc cref="IFileSystem.AppPackageFileExistsAsync(string)"/>
		public Task<bool> AppPackageFileExistsAsync(string filename)
			=> PlatformAppPackageFileExistsAsync(filename);
	}

	static class FileMimeTypes
	{
		internal const string All = "*/*";

		internal const string ImageAll = "image/*";
		internal const string ImagePng = "image/png";
		internal const string ImageJpg = "image/jpeg";

		internal const string VideoAll = "video/*";

		internal const string EmailMessage = "message/rfc822";

		internal const string Pdf = "application/pdf";

		internal const string TextPlain = "text/plain";

		internal const string OctetStream = "application/octet-stream";
	}

	static class FileExtensions
	{
		internal const string Png = ".png";
		internal const string Jpg = ".jpg";
		internal const string Jpeg = ".jpeg";
		internal const string Gif = ".gif";
		internal const string Bmp = ".bmp";

		internal const string Avi = ".avi";
		internal const string Flv = ".flv";
		internal const string Gifv = ".gifv";
		internal const string Mp4 = ".mp4";
		internal const string M4v = ".m4v";
		internal const string Mpg = ".mpg";
		internal const string Mpeg = ".mpeg";
		internal const string Mp2 = ".mp2";
		internal const string Mkv = ".mkv";
		internal const string Mov = ".mov";
		internal const string Qt = ".qt";
		internal const string Wmv = ".wmv";

		internal const string Pdf = ".pdf";

		internal static string[] AllImage =>
			new[] { Png, Jpg, Jpeg, Gif, Bmp };

		internal static string[] AllJpeg =>
			new[] { Jpg, Jpeg };

		internal static string[] AllVideo =>
			new[] { Mp4, Mov, Avi, Wmv, M4v, Mpg, Mpeg, Mp2, Mkv, Flv, Gifv, Qt };

		internal static string Clean(string extension, bool trimLeadingPeriod = false)
		{
			if (string.IsNullOrWhiteSpace(extension))
				return string.Empty;

			extension = extension.TrimStart('*');
			extension = extension.TrimStart('.');

			if (!trimLeadingPeriod)
				extension = "." + extension;

			return extension;
		}
	}

	/// <summary>
	/// A representation of a file and its content type.
	/// </summary>
	public abstract partial class FileBase
	{
		internal const string DefaultContentType = FileMimeTypes.OctetStream;

		string? contentType;

		// The caller must setup FullPath at least!!!
		internal FileBase()
		{
		}

		internal FileBase(string fullPath)
		{
			if (fullPath == null)
				throw new ArgumentNullException(nameof(fullPath));
			if (string.IsNullOrWhiteSpace(fullPath))
				throw new ArgumentException("The file path cannot be an empty string.", nameof(fullPath));
			if (string.IsNullOrWhiteSpace(Path.GetFileName(fullPath)))
				throw new ArgumentException("The file path must be a file path.", nameof(fullPath));

			FullPath = fullPath;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileBase"/> class from an existing instance.
		/// </summary>
		/// <param name="file">A <see cref="FileBase"/> instance that will be used to clone.</param>
		public FileBase(FileBase file)
		{
			FullPath = file.FullPath;
			ContentType = file.ContentType;
			FileName = file.FileName;
			PlatformInit(file);
		}

		internal FileBase(string fullPath, string contentType)
			: this(fullPath)
		{
			FullPath = fullPath;
			ContentType = contentType;
		}

		/// <summary>
		/// Gets the full path and filename.
		/// </summary>
		public string FullPath { get; internal set; } = null!;

		/// <summary>
		/// Gets or sets the file's content type as a MIME type (e.g.: <c>image/png</c>).
		/// </summary>
		public string ContentType
		{
			get => GetContentType();
			set => contentType = value;
		}

		internal string GetContentType()
		{
			// try the provided type
			if (!string.IsNullOrWhiteSpace(contentType))
				return contentType!;

			// try get from the file extension
			var ext = Path.GetExtension(FullPath);
			if (!string.IsNullOrWhiteSpace(ext))
			{
				var content = PlatformGetContentType(ext);
				if (!string.IsNullOrWhiteSpace(content))
					return content;
			}

			return DefaultContentType;
		}

		string? fileName;

		/// <summary>
		/// Gets or sets the filename for this file.
		/// </summary>
		public string FileName
		{
			get => GetFileName();
			set => fileName = value;
		}

		internal string GetFileName()
		{
			// try the provided file name
			if (!string.IsNullOrWhiteSpace(fileName))
				return fileName!;

			// try get from the path
			if (!string.IsNullOrWhiteSpace(FullPath))
				return Path.GetFileName(FullPath);

			// this should never happen as the path is validated in the constructor
			throw new InvalidOperationException($"Unable to determine the file name from '{FullPath}'.");
		}

		/// <summary>
		/// Opens a <see cref="Stream"/> to the corresponding file on the filesystem.
		/// </summary>
		/// <returns>A <see cref="Stream"/> containing the file data.</returns>
		public Task<Stream> OpenReadAsync()
			=> PlatformOpenReadAsync();
	}

	/// <summary>
	/// A representation of a file, that is read-only, and its content type.
	/// </summary>
	public class ReadOnlyFile : FileBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyFile"/> class from a file path.
		/// </summary>
		/// <param name="fullPath">Full file path to the corresponding file on the filesystem.</param>
		public ReadOnlyFile(string fullPath)
			: base(fullPath)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyFile"/> class from a file path, explicitly specifying the content type.
		/// </summary>
		/// <param name="fullPath">Full file path to the corresponding file on the filesystem.</param>
		/// <param name="contentType">Content type (MIME type) of the file (e.g.: <c>image/png</c>).</param>
		public ReadOnlyFile(string fullPath, string contentType)
			: base(fullPath, contentType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyFile"/> class from an existing instance.
		/// </summary>
		/// <param name="file">A <see cref="FileBase"/> instance that will be used to clone.</param>
		public ReadOnlyFile(FileBase file)
			: base(file)
		{
		}
	}

	/// <summary>
	/// A representation of a file, as a result of a pick action by the user, and its content type.
	/// </summary>
	public partial class FileResult : FileBase
	{
		// The caller must setup FullPath at least!!!
		internal FileResult()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileResult"/> class from a file path.
		/// </summary>
		/// <param name="fullPath">Full file path to the corresponding file on the filesystem.</param>
		public FileResult(string fullPath)
			: base(fullPath)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileResult"/> class from a file path, explicitly specifying the content type.
		/// </summary>
		/// <param name="fullPath">Full file path to the corresponding file on the filesystem.</param>
		/// <param name="contentType">Content type (MIME type) of the file (e.g.: <c>image/png</c>).</param>
		public FileResult(string fullPath, string contentType)
			: base(fullPath, contentType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileResult"/> class from an existing instance.
		/// </summary>
		/// <param name="file">A <see cref="FileBase"/> instance that will be used to clone.</param>
		public FileResult(FileBase file)
			: base(file)
		{
		}
	}
}