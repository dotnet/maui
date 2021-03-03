using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Maui.Controls.Internals;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{

	[Preserve(AllMembers = true)]
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		public (bool success, string filePath) LoadFont(EmbeddedFont font)
		{
			var tmpdir = IOPath.GetTempPath();
			var filePath = IOPath.Combine(tmpdir, font.FontName);
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
	}
}
