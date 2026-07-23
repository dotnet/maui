using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// An <see cref="IFileProvider"/> that overlays a single in-memory host page (the rendered
	/// <see cref="BlazorWebView.AppType"/> document) on top of the platform's physical file provider.
	/// Requests for the host page relative path are served from memory; everything else is delegated
	/// to the inner provider so static assets continue to be served from disk.
	/// </summary>
	internal sealed class HostPageFileProvider : IFileProvider
	{
		private readonly IFileProvider _inner;
		private readonly string _hostPageRelativePath;
		private readonly byte[] _contents;

		public HostPageFileProvider(IFileProvider inner, string hostPageRelativePath, string html)
		{
			_inner = inner ?? throw new ArgumentNullException(nameof(inner));
			_hostPageRelativePath = NormalizePath(hostPageRelativePath);
			_contents = Encoding.UTF8.GetBytes(html);
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			if (string.Equals(NormalizePath(subpath), _hostPageRelativePath, StringComparison.Ordinal))
			{
				return new InMemoryFileInfo(Path.GetFileName(_hostPageRelativePath), _contents);
			}

			return _inner.GetFileInfo(subpath);
		}

		public IDirectoryContents GetDirectoryContents(string subpath) => _inner.GetDirectoryContents(subpath);

		public IChangeToken Watch(string filter) => _inner.Watch(filter);

		private static string NormalizePath(string path) =>
			(path ?? string.Empty).Replace('\\', '/').TrimStart('/');

		private sealed class InMemoryFileInfo : IFileInfo
		{
			private readonly byte[] _contents;

			public InMemoryFileInfo(string name, byte[] contents)
			{
				Name = name;
				_contents = contents;
			}

			public bool Exists => true;
			public long Length => _contents.Length;
			public string? PhysicalPath => null;
			public string Name { get; }
			public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
			public bool IsDirectory => false;

			public Stream CreateReadStream() => new MemoryStream(_contents, writable: false);
		}
	}
}
