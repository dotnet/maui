using System;
using System.IO;
using ElmSharp;
using IOPath = System.IO.Path;
using TApplication = Tizen.Applications.Application;

namespace Xamarin.Forms.Platform.Tizen
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader, IRegisterable
	{
		const string _fontCacheFolderName = "fonts";

		public DirectoryInfo FontCacheDirectory { get; private set; }

		public EmbeddedFontLoader()
		{
			FontCacheDirectory = Directory.CreateDirectory(IOPath.Combine(TApplication.Current.DirectoryInfo.Data, _fontCacheFolderName));
			Utility.AppendGlobalFontPath(FontCacheDirectory.FullName);
		}

		public (bool success, string filePath) LoadFont(EmbeddedFont font)
		{
			var filePath = IOPath.Combine(FontCacheDirectory.FullName, font.FontName);
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
				Log.Error(ex.Message);
				File.Delete(filePath);
			}
			return (false, null);
		}
	}
}