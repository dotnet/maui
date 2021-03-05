using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Maui
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		public (bool success, string? filePath) LoadFont(EmbeddedFont font)
		{
			var tmpdir = Path.GetTempPath();
			var filePath = Path.Combine(tmpdir, font.FontName!);
			if (File.Exists(filePath))
				return (true, filePath);

			try
			{
				if (font.ResourceStream == null)
					throw new InvalidOperationException("ResourceStream was null.");

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
	}
}