#nullable enable
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Windows.Storage;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial class EmbeddedFontLoader
	{
		const string FontCacheFolderName = "fonts";

		/// <inheritdoc/>
		public string? LoadFont(EmbeddedFont font)
		{
			var tmpdir = ApplicationData.Current.LocalFolder.CreateFolderAsync(FontCacheFolderName, CreationCollisionOption.OpenIfExists).AsTask().Result;

			var file = tmpdir.TryGetItemAsync(font.FontName).AsTask().Result;
			if (file != null)
				return CleanseFilePath(file.Path);

			StorageFile? newFile = null;
			try
			{
				if (font.ResourceStream == null)
					throw new InvalidOperationException("ResourceStream was null.");

				newFile = tmpdir.CreateFileAsync(font.FontName).AsTask().Result;
				using (var fileStream = newFile.OpenStreamForWriteAsync().Result)
				{
					font.ResourceStream.CopyTo(fileStream);
				}

				return CleanseFilePath(newFile.Path);
			}
			catch (Exception ex)
			{
				_serviceProvider?.CreateLogger<FontManager>()?.LogWarning(ex, "Unable copy font {Font} to local file system.", font.FontName);

				if (newFile != null)
					newFile.DeleteAsync().AsTask().Wait();
			}

			return null;
		}

		static string CleanseFilePath(string filePath)
		{
			var fontName = Path.GetFileName(filePath);

			filePath = Path.Combine("local", FontCacheFolderName, fontName);

			var baseUri = new Uri("ms-appdata://");
			var uri = new Uri(baseUri, filePath);
			var relativePath = uri.ToString().TrimEnd('/');

			return relativePath;
		}
	}
}