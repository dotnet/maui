using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
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
		// Static mapping for file extensions to MIME types
		// Used as fallback when Windows StorageFile.ContentType doesn't provide correct MIME type
		static readonly FrozenDictionary<string, string> ExtensionToMimeTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			// Image formats
			{ ".jpg", MediaTypeNames.Image.Jpeg },
			{ ".jpeg", MediaTypeNames.Image.Jpeg },
			{ ".png", "image/png" },
			{ ".gif", MediaTypeNames.Image.Gif },
			{ ".bmp", "image/bmp" },
			{ ".svg", "image/svg+xml" },
			{ ".webp", "image/webp" },
			{ ".tiff", MediaTypeNames.Image.Tiff },
			{ ".tif", MediaTypeNames.Image.Tiff },
			{ ".ico", "image/x-icon" },
			
			// Audio formats
			{ ".mp3", "audio/mpeg" },
			{ ".wav", "audio/wav" },
			{ ".flac", "audio/flac" },
			{ ".aac", "audio/aac" },
			{ ".ogg", "audio/ogg" },
			{ ".wma", "audio/x-ms-wma" },
			
			// Video formats
			{ ".mp4", "video/mp4" },
			{ ".avi", "video/x-msvideo" },
			{ ".mov", "video/quicktime" },
			{ ".wmv", "video/x-ms-wmv" },
			{ ".webm", "video/webm" },
			{ ".mkv", "video/x-matroska" },
			{ ".flv", "video/x-flv" },
			
			// Document formats
			{ ".pdf", MediaTypeNames.Application.Pdf },
			{ ".doc", "application/msword" },
			{ ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
			{ ".xls", "application/vnd.ms-excel" },
			{ ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
			{ ".ppt", "application/vnd.ms-powerpoint" },
			{ ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
			{ ".txt", MediaTypeNames.Text.Plain },
			{ ".rtf", MediaTypeNames.Application.Rtf },
			
			// Web formats
			{ ".html", MediaTypeNames.Text.Html },
			{ ".htm", MediaTypeNames.Text.Html },
			{ ".css", "text/css" },
			{ ".js", "application/javascript" },
			{ ".json", "application/json" },
			{ ".xml", MediaTypeNames.Text.Xml },
			
			// Archive formats
			{ ".zip", MediaTypeNames.Application.Zip },
			{ ".rar", "application/x-rar-compressed" },
			{ ".7z", "application/x-7z-compressed" },
			{ ".tar", "application/x-tar" },
			{ ".tar.gz", "application/gzip" },
			{ ".gz", "application/gzip" }
		}.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

		internal FileBase(IStorageFile file)
			: this(file?.Path)
		{
			File = file;
			
			// Set the ContentType, but prefer our mapping for known problematic extensions
			var fileContentType = file?.ContentType;
			var extension = Path.GetExtension(file?.Path ?? "");
			
			// If Windows provides a generic "application/octet-stream" or empty ContentType,
			// try to get a more specific MIME type from our extension mapping
			if (string.IsNullOrWhiteSpace(fileContentType) || 
			    fileContentType == "application/octet-stream")
			{
				var betterContentType = PlatformGetContentType(extension);
				ContentType = betterContentType ?? fileContentType;
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

		// Use extension mapping as fallback when Windows doesn't provide correct MIME type
		string PlatformGetContentType(string extension)
		{
			if (string.IsNullOrWhiteSpace(extension))
				return null;

			// Trim whitespace and convert to lowercase for consistent matching
			extension = extension.Trim().ToLowerInvariant();
			if (string.IsNullOrEmpty(extension))
				return null;

			if (!extension.StartsWith("."))
				extension = "." + extension;

			// Try exact match first
			if (ExtensionToMimeTypeMap.TryGetValue(extension, out var mimeType))
				return mimeType;

			// For compound extensions like .tar.gz, try to match longer extensions first
			// This handles cases where we have both .gz and .tar.gz in our mapping
			var fileName = Path.GetFileNameWithoutExtension(extension);
			if (!string.IsNullOrEmpty(fileName) && fileName.Contains('.', StringComparison.Ordinal))
			{
				// Try progressively longer extensions (e.g., .tar.gz, then .gz)
				var parts = fileName.Split('.');
				for (int i = parts.Length - 1; i >= 0; i--)
				{
					var compoundExtension = "." + string.Join(".", parts.Skip(i)) + extension;
					if (ExtensionToMimeTypeMap.TryGetValue(compoundExtension, out mimeType))
						return mimeType;
				}
			}

			return null;
		}

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
