using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Wraps the platform's physical file provider to add hybrid host-page and static web asset
	/// behaviour on top of it:
	/// <list type="bullet">
	/// <item><description>serves the in-memory rendered host page (the <see cref="BlazorWebView.AppType"/>
	/// document) at the host page path, when provided;</description></item>
	/// <item><description>resolves fingerprinted request routes (for example <c>app.abc123.css</c>) to
	/// their physical asset files, when a manifest is provided; and</description></item>
	/// <item><description>hides the bundled asset manifest from being served to the web view.</description></item>
	/// </list>
	/// All other requests are delegated unchanged, so existing behaviour is preserved.
	/// </summary>
	internal sealed class BlazorWebViewFileProvider : IFileProvider
	{
		private readonly IFileProvider _inner;
		private readonly string? _hostPageRelativePath;
		private readonly byte[]? _hostPageContents;
		private readonly StaticWebAssetsManifest? _manifest;

		public BlazorWebViewFileProvider(
			IFileProvider inner,
			string? hostPageRelativePath,
			string? hostPageHtml,
			StaticWebAssetsManifest? manifest)
		{
			_inner = inner ?? throw new ArgumentNullException(nameof(inner));
			_manifest = manifest;

			if (hostPageRelativePath is not null && hostPageHtml is not null)
			{
				_hostPageRelativePath = NormalizePath(hostPageRelativePath);
				_hostPageContents = Encoding.UTF8.GetBytes(hostPageHtml);
			}
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			var normalized = NormalizePath(subpath);

			// Serve the rendered host page from memory.
			if (_hostPageContents is not null &&
				string.Equals(normalized, _hostPageRelativePath, StringComparison.Ordinal))
			{
				return new InMemoryFileInfo(Path.GetFileName(_hostPageRelativePath!), _hostPageContents);
			}

			// Never expose the bundled asset manifest to the web view.
			if (string.Equals(normalized, StaticWebAssetsManifest.ManifestRelativePath, StringComparison.OrdinalIgnoreCase))
			{
				return new NotFoundFileInfo(subpath);
			}

			// If the file exists as requested, serve it directly (preserves existing behaviour).
			var fileInfo = _inner.GetFileInfo(subpath);
			if (fileInfo.Exists)
			{
				return fileInfo;
			}

			// Otherwise, if the request targets a fingerprinted route, serve the physical asset.
			if (_manifest is not null &&
				_manifest.TryResolvePhysicalPath(normalized, out var physicalPath))
			{
				return _inner.GetFileInfo(physicalPath);
			}

			return fileInfo;
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
