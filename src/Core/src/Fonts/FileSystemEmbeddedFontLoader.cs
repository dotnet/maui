#nullable enable
using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public class FileSystemEmbeddedFontLoader : IEmbeddedFontLoader
	{
		readonly string _rootPath;
		readonly ILogger<EmbeddedFontLoader>? _logger;

		public FileSystemEmbeddedFontLoader(string rootPath, ILogger<EmbeddedFontLoader>? logger = null)
		{
			_rootPath = rootPath;
			_logger = logger;
		}

		public string? LoadFont(EmbeddedFont font)
		{
			var filePath = Path.Combine(_rootPath, font.FontName!);
			if (File.Exists(filePath))
				return filePath;

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

				return filePath;
			}
			catch (Exception ex)
			{
				_logger?.LogWarning(ex, "Unable copy font {Font} to local file system.", font.FontName);

				File.Delete(filePath);
			}

			return null;
		}
	}
}