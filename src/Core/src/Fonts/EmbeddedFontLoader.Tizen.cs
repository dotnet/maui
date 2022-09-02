using System;
using System.Diagnostics;
using System.IO;
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
				Tizen.NUI.FontClient.Instance.AddCustomFontDirectory(FontCacheDirectory.FullName);
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