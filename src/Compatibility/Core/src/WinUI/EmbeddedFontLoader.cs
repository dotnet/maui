using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		const string _fontCacheFolderName = "fonts";
		public (bool success, string filePath) LoadFont(EmbeddedFont font)
		{
			try
			{
				var t = ApplicationData.Current.LocalFolder.CreateFolderAsync(_fontCacheFolderName, CreationCollisionOption.OpenIfExists);
				var tmpdir = t.AsTask().Result;

				var file = tmpdir.TryGetItemAsync(font.FontName).AsTask().Result;
				string filePath = "";
				if (file != null)
				{
					filePath = file.Path;
					return (true, CleanseFilePath(filePath));
				}

				try
				{

					var f = tmpdir.CreateFileAsync(font.FontName).AsTask().Result;
					filePath = f.Path;
					using (var fileStream = File.Open(f.Path, FileMode.Open))
					{
						font.ResourceStream.CopyTo(fileStream);
					}
					return (true, CleanseFilePath(filePath));
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					File.Delete(filePath);
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}
			return (false, null);
		}

		static string CleanseFilePath(string filePath)
		{
			var fontName = IOPath.GetFileName(filePath);
			filePath = IOPath.Combine("local", _fontCacheFolderName, fontName);
			var baseUri = new Uri("ms-appdata://");
			var uri = new Uri(baseUri, filePath);
			var relativePath = uri.ToString().TrimEnd('/');
			return relativePath;
		}
	}
}
