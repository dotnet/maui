using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Microsoft.Maui
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		const string FontCacheFolderName = "fonts";

		public (bool success, string? filePath) LoadFont(EmbeddedFont font) =>
			LoadFontAsync(font).Result;

		public async Task<(bool success, string? filePath)> LoadFontAsync(EmbeddedFont font)
		{
			var tmpdir = await ApplicationData.Current.LocalFolder.CreateFolderAsync(FontCacheFolderName, CreationCollisionOption.OpenIfExists);

			var file = await tmpdir.TryGetItemAsync(font.FontName);
			if (file != null)
				return (true, CleanseFilePath(file.Path));

			StorageFile? newFile = null;
			try
			{
				if (font.ResourceStream == null)
					throw new InvalidOperationException("ResourceStream was null.");

				newFile = await tmpdir.CreateFileAsync(font.FontName);
				using (var fileStream = await newFile.OpenStreamForReadAsync())
				{
					font.ResourceStream.CopyTo(fileStream);
				}

				return (true, CleanseFilePath(newFile.Path));
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);

				if (newFile != null)
					await newFile.DeleteAsync();
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