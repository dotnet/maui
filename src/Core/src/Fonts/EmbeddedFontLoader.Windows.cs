#nullable enable
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Windows.Storage;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial class EmbeddedFontLoader
	{
		const string FontCacheFolderName = "Fonts";

		/// <inheritdoc/>
		public string? LoadFont(EmbeddedFont font)
		{
			if (string.IsNullOrWhiteSpace(font.FontName))
			{
				throw new InvalidOperationException("FontName for embedded font was null or empty.");
			}
			if (font.ResourceStream is null)
			{
				throw new InvalidOperationException("ResourceStream for embedded font was null.");
			}

			return AppInfoUtils.IsPackagedApp
				? LoadFontPackaged(font.FontName, font.ResourceStream)
				: LoadFontUnpackaged(font.FontName, font.ResourceStream);
		}

		private string? LoadFontPackaged(string fontName, Stream resourceStream)
		{
			var tmpdir = ApplicationData.Current.TemporaryFolder.CreateFolderAsync(FontCacheFolderName, CreationCollisionOption.OpenIfExists).AsTask().Result;

			// return an existing file from the cache
			var existingFile = tmpdir.TryGetItemAsync(fontName).AsTask().Result;
			if (existingFile is not null)
			{
				return CleansePackagedFilePath(existingFile.Path);
			}

			// copy the file into the cache
			StorageFile? newFile = null;
			try
			{
				newFile = tmpdir.CreateFileAsync(fontName).AsTask().Result;
				using (var fileStream = newFile.OpenStreamForWriteAsync().Result)
				{
					resourceStream.CopyTo(fileStream);
				}

				return CleansePackagedFilePath(newFile.Path);
			}
			catch (Exception ex)
			{
				_serviceProvider?.CreateLogger<FontManager>()?.LogWarning(ex, "Unable to copy font {Font} to the local file system.", fontName);
			}

			// something went wrong!

			// first, clean up the font just in case we wrote some bad data
			if (newFile is not null)
			{
				try
				{
					newFile.DeleteAsync().AsTask().Wait();
				}
				catch (Exception ex)
				{
					_serviceProvider?.CreateLogger<FontManager>()?.LogWarning(ex, "Unable to delete font {Font} from local file system.", fontName);
				}
			}

			// then, return null
			return null;
		}

		static string CleansePackagedFilePath(string filePath)
		{
			var fontName = Path.GetFileName(filePath);

			filePath = Path.Combine("temp", FontCacheFolderName, fontName);

			var baseUri = new Uri("ms-appdata://");
			var uri = new Uri(baseUri, filePath);
			var relativePath = uri.ToString().TrimEnd('/');

			return relativePath;
		}

		static string CleanseUnpackagedFilePath(string filePath)
		{
			// yes, ms-appx, yes, it is the way... sometimes #acceptance
			//
			// The ms-appx prefix is used for fonts, regardless of the location,
			// whether in the package or in the local file system.
			// The times it is not used is when it is in another, special
			// location, such as in the app data.

			var baseUri = new Uri("ms-appx://");
			var uri = new Uri(baseUri, "ms-appx://" + filePath);
			var relativePath = uri.ToString().TrimEnd('/');

			return relativePath;
		}

		private string? LoadFontUnpackaged(string fontName, Stream resourceStream)
		{
			var tmpdir = Path.Combine(FileSystem.CacheDirectory, "..", FontCacheFolderName);
			tmpdir = Path.GetFullPath(tmpdir);

			// return an existing file from the cache
			var file = Path.Combine(tmpdir, fontName);
			if (File.Exists(file))
			{
				return CleanseUnpackagedFilePath(file);
			}

			// copy the file into the cache
			try
			{
				Directory.CreateDirectory(tmpdir);
				using (var fileStream = File.Create(file))
				{
					resourceStream.CopyTo(fileStream);
				}

				return CleanseUnpackagedFilePath(file);
			}
			catch (Exception ex)
			{
				_serviceProvider?.CreateLogger<FontManager>()?.LogWarning(ex, "Unable to copy font {Font} to the local file system.", fontName);
			}

			// something went wrong!

			// first, clean up the font just in case we wrote some bad data
			try
			{
				if (File.Exists(file))
				{
					File.Delete(file);
				}
			}
			catch (Exception ex)
			{
				_serviceProvider?.CreateLogger<FontManager>()?.LogWarning(ex, "Unable to delete font {Font} from local file system.", fontName);
			}

			// then, return null
			return null;
		}
	}
}