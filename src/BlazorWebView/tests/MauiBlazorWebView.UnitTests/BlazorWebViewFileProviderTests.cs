using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.UnitTests
{
	public class BlazorWebViewFileProviderTests
	{
		static StaticWebAssetsManifest ParseManifest(string json)
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
			return StaticWebAssetsManifest.Parse(stream);
		}

		[Fact]
		public void RenderedHostPageIsServedFromMemory()
		{
			var provider = new BlazorWebViewFileProvider(new FakeFileProvider(), "wwwroot/index.html", "<html>hi</html>", manifest: null);

			var fileInfo = provider.GetFileInfo("wwwroot/index.html");

			Assert.True(fileInfo.Exists);
			using var reader = new StreamReader(fileInfo.CreateReadStream());
			Assert.Equal("<html>hi</html>", reader.ReadToEnd());
		}

		[Fact]
		public void NullHostPageHtmlFallsThroughAndIsNotServed()
		{
			// This is the "silent blank page" path: when the AppType render never produced HTML
			// (hostPageHtml == null) the host page must fall through to the inner provider, which here
			// has no physical index.html, so the request is genuinely not found.
			var provider = new BlazorWebViewFileProvider(new FakeFileProvider(), "wwwroot/index.html", hostPageHtml: null, manifest: null);

			var fileInfo = provider.GetFileInfo("wwwroot/index.html");

			Assert.False(fileInfo.Exists);
		}

		[Fact]
		public void ExistingFileIsServedDirectly()
		{
			var provider = new BlazorWebViewFileProvider(new FakeFileProvider("real.js"), hostPageRelativePath: null, hostPageHtml: null, manifest: null);

			Assert.True(provider.GetFileInfo("real.js").Exists);
		}

		[Fact]
		public void FingerprintedRouteResolvesToPhysicalAsset()
		{
			var manifest = ParseManifest("{\"Assets\":[{\"Url\":\"app.abc123.css\",\"Properties\":[{\"Name\":\"label\",\"Value\":\"app.css\"}]}]}");
			var provider = new BlazorWebViewFileProvider(new FakeFileProvider("app.css"), hostPageRelativePath: null, hostPageHtml: null, manifest);

			var fileInfo = provider.GetFileInfo("app.abc123.css");

			Assert.True(fileInfo.Exists);
		}

		[Fact]
		public void UnknownRequestWithoutManifestIsNotFound()
		{
			var provider = new BlazorWebViewFileProvider(new FakeFileProvider(), hostPageRelativePath: null, hostPageHtml: null, manifest: null);

			Assert.False(provider.GetFileInfo("missing.css").Exists);
		}

		[Fact]
		public void RenderedHostPageLastModifiedIsStable()
		{
			var provider = new BlazorWebViewFileProvider(new FakeFileProvider(), "wwwroot/index.html", "<html/>", manifest: null);

			var fileInfo = provider.GetFileInfo("wwwroot/index.html");

			Assert.Equal(fileInfo.LastModified, fileInfo.LastModified);
		}

		sealed class FakeFileProvider : IFileProvider
		{
			readonly HashSet<string> _existing;

			public FakeFileProvider(params string[] existing) =>
				_existing = new HashSet<string>(existing, StringComparer.Ordinal);

			public IFileInfo GetFileInfo(string subpath)
			{
				var normalized = subpath.Replace('\\', '/').TrimStart('/');
				return new FakeFileInfo(normalized, _existing.Contains(normalized));
			}

			public IDirectoryContents GetDirectoryContents(string subpath) => throw new NotSupportedException();

			public IChangeToken Watch(string filter) => throw new NotSupportedException();
		}

		sealed class FakeFileInfo : IFileInfo
		{
			public FakeFileInfo(string name, bool exists)
			{
				Name = name;
				Exists = exists;
			}

			public bool Exists { get; }
			public long Length => 0;
			public string? PhysicalPath => null;
			public string Name { get; }
			public DateTimeOffset LastModified => DateTimeOffset.UnixEpoch;
			public bool IsDirectory => false;

			public Stream CreateReadStream() => new MemoryStream();
		}
	}
}
