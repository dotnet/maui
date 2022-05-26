using System;
using System.Diagnostics;
using System.IO;
using ElmSharp;
using Tizen.Common;
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
			var name = IOPath.GetFileNameWithoutExtension(filePath);
			if (File.Exists(filePath))
				return name;
			try
			{
				using (var fileStream = File.Create(filePath))
				{
					if (font.ResourceStream == null)
						throw new InvalidOperationException("ResourceStream was null.");

					font.ResourceStream.CopyTo(fileStream);
				}

#if __TIZEN__
				if (DotnetUtil.TizenAPIVersion > 5)
				{
					Utility.FontReinit();
				}
#endif
				return name;
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