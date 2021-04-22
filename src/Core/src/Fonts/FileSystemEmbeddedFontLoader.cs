#nullable enable
using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Maui
{
	public class FileSystemEmbeddedFontLoader : IEmbeddedFontLoader
	{
		readonly string _rootPath;

		public FileSystemEmbeddedFontLoader(string rootPath)
		{
			_rootPath = rootPath;
		}

		public (bool success, string? filePath) LoadFont(EmbeddedFont font)
		{
			var filePath = Path.Combine(_rootPath, font.FontName!);
			if (File.Exists(filePath))
				return (true, filePath);

			try
			{
				if (font.ResourceStream == null)
					throw new InvalidOperationException("ResourceStream was null.");

				if (!Directory.Exists(_rootPath))
					Directory.CreateDirectory(_rootPath);

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