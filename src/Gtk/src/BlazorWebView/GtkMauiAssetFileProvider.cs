using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// An <see cref="IFileProvider"/> over the app package files of a Linux app (the folder of the executable, where the
	/// <c>wwwroot</c> content is copied to), rooted at a content directory. Only files are served; paths that leave the root are refused.
	/// </summary>
	internal sealed class GtkMauiAssetFileProvider : IFileProvider
	{
		readonly string _root;

		/// <summary>Creates the provider.</summary>
		/// <param name="contentRootDir">The content directory, relative to the app package directory.</param>
		/// <param name="packageDirectory">The app package directory; <see cref="AppContext.BaseDirectory"/> when <see langword="null"/>.</param>
		public GtkMauiAssetFileProvider(string contentRootDir, string? packageDirectory = null)
		{
			_root = Path.GetFullPath(Path.Combine(packageDirectory ?? AppContext.BaseDirectory, contentRootDir ?? string.Empty));
		}

		/// <summary>Resolves <paramref name="subpath"/> to a full path inside the root, or <see langword="null"/> when it leaves the root.</summary>
		internal string? Resolve(string? subpath)
		{
			var relative = (subpath ?? string.Empty).Replace('\\', '/').TrimStart('/');
			if (relative.Length == 0)
				return null;

			foreach (var segment in relative.Split('/'))
			{
				if (segment == "..")
					return null;
			}

			var rootWithSeparator = _root.EndsWith(Path.DirectorySeparatorChar) ? _root : _root + Path.DirectorySeparatorChar;
			var full = Path.GetFullPath(Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar)));

			// Linux file names are case-sensitive.
			var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			return full.StartsWith(rootWithSeparator, comparison) ? full : null;
		}

		/// <inheritdoc/>
		public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;

		/// <inheritdoc/>
		public IFileInfo GetFileInfo(string subpath)
		{
			var path = Resolve(subpath);
			return path is null ? new NotFoundFileInfo(subpath) : new PhysicalFile(path);
		}

		/// <inheritdoc/>
		public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

		sealed class PhysicalFile : IFileInfo
		{
			readonly string _path;

			public PhysicalFile(string path)
			{
				_path = path;
				Name = Path.GetFileName(path);

				var info = new FileInfo(path);
				Exists = info.Exists;
				Length = Exists ? info.Length : -1;
			}

			public bool Exists { get; }

			public long Length { get; }

			public string? PhysicalPath => null;

			public string Name { get; }

			public DateTimeOffset LastModified { get; } = DateTimeOffset.FromUnixTimeSeconds(0);

			public bool IsDirectory => false;

			public Stream CreateReadStream() => File.OpenRead(_path);
		}
	}
}
