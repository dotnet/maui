using System;
using System.IO;
using ElmSharp;
using Tizen.Common;
using IOPath = System.IO.Path;
using TApplication = Tizen.Applications.Application;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
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

				if (DotnetUtil.TizenAPIVersion > 5)
				{
					FontExtensions.FontReinit();
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