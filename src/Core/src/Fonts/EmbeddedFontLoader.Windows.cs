using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;

namespace Microsoft.Maui
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		const string FontCacheFolderName = "fonts";

		public (bool success, string? filePath) LoadFont(EmbeddedFont font)
		{
			var tmpdir = ApplicationData.Current.LocalFolder.CreateFolderAsync(FontCacheFolderName, CreationCollisionOption.OpenIfExists).AsTask().Result;

			var file = tmpdir.TryGetItemAsync(font.FontName).AsTask().Result;
			if (file != null)
				return (true, CleanseFilePath(file.Path));

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

				return (true, CleanseFilePath(newFile.Path));
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);

				if (newFile != null)
					newFile.DeleteAsync().AsTask().Wait();
			}

			return (false, null);
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