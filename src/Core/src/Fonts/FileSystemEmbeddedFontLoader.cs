#nullable enable
using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public class FileSystemEmbeddedFontLoader : IEmbeddedFontLoader
	{
		readonly Lazy<string> _rootPath;
		readonly ILogger<EmbeddedFontLoader>? _logger;

		public FileSystemEmbeddedFontLoader(string rootPath, ILogger<EmbeddedFontLoader>? logger = null)
		{
			_rootPath = new Lazy<string>(() => rootPath);
			_logger = logger;
		}

		public FileSystemEmbeddedFontLoader(Func<string> getRootPath, ILogger<EmbeddedFontLoader>? logger = null)
		{
			_rootPath = new Lazy<string>(getRootPath);
			_logger = logger;
		}

		string RootPath => _rootPath.Value;

		public string? LoadFont(EmbeddedFont font)
		{
			var filePath = Path.Combine(RootPath, font.FontName!);
			if (File.Exists(filePath))
				return filePath;

			try
			{
				if (font.ResourceStream == null)
					throw new InvalidOperationException("ResourceStream was null.");

				if (!Directory.Exists(RootPath))
					Directory.CreateDirectory(RootPath);

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