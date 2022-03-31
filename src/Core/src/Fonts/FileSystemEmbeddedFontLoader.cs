#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public class FileSystemEmbeddedFontLoader : IEmbeddedFontLoader
	{
		string? _rootPath;
		readonly Func<string>? _getRootPath;
		readonly IServiceProvider? _serviceProvider;

		public FileSystemEmbeddedFontLoader(string rootPath, IServiceProvider? serviceProvider = null)
		{
			_rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
			_serviceProvider = serviceProvider;
		}

		// Allows for delay-loading _rootPath, in case it is expensive and isn't always used.
		private protected FileSystemEmbeddedFontLoader(Func<string> getRootPath, IServiceProvider? serviceProvider = null)
		{
			_getRootPath = getRootPath;
			_serviceProvider = serviceProvider;
		}

		private string RootPath
		{
			get
			{
				Debug.Assert(_rootPath != null || _getRootPath != null, $"The ctor should have set either {nameof(_rootPath)} or {nameof(_getRootPath)}.");
				
				return _rootPath ??= _getRootPath!();
			}
		}

		public string? LoadFont(EmbeddedFont font)
		{
			string rootPath = RootPath;
			var filePath = Path.Combine(rootPath, font.FontName!);
			if (File.Exists(filePath))
				return filePath;

			try
			{
				if (font.ResourceStream == null)
					throw new InvalidOperationException("ResourceStream was null.");

				if (!Directory.Exists(rootPath))
					Directory.CreateDirectory(rootPath);

				using (var fileStream = File.Create(filePath))
				{
					font.ResourceStream.CopyTo(fileStream);
				}

				return filePath;
			}
			catch (Exception ex)
			{
				_serviceProvider?.CreateLogger<EmbeddedFontLoader>()?.LogWarning(ex, "Unable copy font {Font} to local file system.", font.FontName);

				File.Delete(filePath);
			}

			return null;
		}
	}
}