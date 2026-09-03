using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		public (bool success, string filePath) LoadFont(EmbeddedFont font)
		{
			var tmpdir = Path.Combine(Path.GetTempPath(), System.Reflection.Assembly.GetEntryAssembly().GetName().Name, "Fonts");
			Directory.CreateDirectory(tmpdir);
			var filePath = Path.Combine(tmpdir, font.FontName);
			if (File.Exists(filePath))
				return (true, filePath);
			try
			{
				using (var fileStream = File.Create(filePath))
				{
					font.ResourceStream.CopyTo(fileStream);
				}
				return (true, filePath);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				File.Delete(filePath);
			}
			return (false, null);
		}

		//static string CleanseFilePath(string filePath)
		//{
		//	var fontName = Path.GetFileName(filePath);
		//	filePath = Path.Combine("local", _fontCacheFolderName, fontName);
		//	var baseUri = new Uri("ms-appdata://");
		//	var uri = new Uri(baseUri, filePath);
		//	var relativePath = uri.ToString().TrimEnd('/');
		//	return relativePath;
		//}
	}
}
