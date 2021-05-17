using System;
using System.Diagnostics;
using System.IO;
using ElmSharp;
using IOPath = System.IO.Path;
using TApplication = Tizen.Applications.Application;

namespace Microsoft.Maui
{
	public partial class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		const string _fontCacheFolderName = "fonts";

		public DirectoryInfo? FontCacheDirectory { get; private set; }

		public string? LoadFont(EmbeddedFont font)
		{
			if (FontCacheDirectory == null)
			{ 
				FontCacheDirectory = Directory.CreateDirectory(IOPath.Combine(TApplication.Current.DirectoryInfo.Data, _fontCacheFolderName));
				Utility.AppendGlobalFontPath(FontCacheDirectory.FullName);
			}

			var filePath = IOPath.Combine(FontCacheDirectory.FullName, font.FontName!);
			if (File.Exists(filePath))
				return filePath;
			try
			{
				using (var fileStream = File.Create(filePath))
				{
					if (font.ResourceStream == null)
						throw new InvalidOperationException("ResourceStream was null.");

					font.ResourceStream.CopyTo(fileStream);
				}

				//TODO: should include below
				//if (DotnetUtil.TizenAPIVersion > 5)
				//{
				//	FontExtensions.FontReinit();
				//}

				return filePath;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				File.Delete(filePath);
			}
			return null;
		}
	}
}