using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.UnitTests
{
	public class StaticWebAssetsManifestTests
	{
		static StaticWebAssetsManifest Parse(string json)
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
			return StaticWebAssetsManifest.Parse(stream);
		}

		[Fact]
		public void EmptyAssetsArrayProducesEmptyMap()
		{
			var manifest = Parse("{\"Assets\":[]}");

			Assert.Empty(manifest.RouteToPhysicalPath);
		}

		[Fact]
		public void MissingAssetsPropertyProducesEmptyMap()
		{
			var manifest = Parse("{}");

			Assert.Empty(manifest.RouteToPhysicalPath);
		}

		[Fact]
		public void FingerprintedAssetResolvesRouteToLabel()
		{
			var manifest = Parse("{\"Assets\":[{\"Route\":\"app.abc123.css\",\"Label\":\"app.css\"}]}");

			Assert.True(manifest.TryResolvePhysicalPath("app.abc123.css", out var physical));
			Assert.Equal("app.css", physical);
		}

		[Fact]
		public void EntriesWithNullRouteOrLabelAreSkipped()
		{
			var manifest = Parse(
				"{\"Assets\":[" +
				"{\"Route\":null,\"Label\":\"a.css\"}," +
				"{\"Route\":\"b.css\",\"Label\":null}," +
				"{\"Route\":\"c.abc.css\",\"Label\":\"c.css\"}]}");

			Assert.Single(manifest.RouteToPhysicalPath);
			Assert.True(manifest.TryResolvePhysicalPath("c.abc.css", out var physical));
			Assert.Equal("c.css", physical);
		}

		[Fact]
		public void DuplicateRoutesResolveFirstWins()
		{
			var manifest = Parse(
				"{\"Assets\":[" +
				"{\"Route\":\"dup.css\",\"Label\":\"first.css\"}," +
				"{\"Route\":\"dup.css\",\"Label\":\"second.css\"}]}");

			Assert.True(manifest.TryResolvePhysicalPath("dup.css", out var physical));
			Assert.Equal("first.css", physical);
		}

		[Fact]
		public void RouteEqualToLabelDoesNotResolve()
		{
			var manifest = Parse("{\"Assets\":[{\"Route\":\"same.css\",\"Label\":\"same.css\"}]}");

			Assert.False(manifest.TryResolvePhysicalPath("same.css", out _));
		}

		[Fact]
		public void LeadingSlashAndBackslashAreNormalized()
		{
			var manifest = Parse("{\"Assets\":[{\"Route\":\"/sub\\\\x.abc.css\",\"Label\":\"/sub/x.css\"}]}");

			Assert.True(manifest.TryResolvePhysicalPath("sub/x.abc.css", out var physical));
			Assert.Equal("sub/x.css", physical);
		}

		[Fact]
		public void UnknownRouteDoesNotResolve()
		{
			var manifest = Parse("{\"Assets\":[{\"Route\":\"app.abc123.css\",\"Label\":\"app.css\"}]}");

			Assert.False(manifest.TryResolvePhysicalPath("other.css", out _));
		}

		[Fact]
		public void MalformedJsonThrows()
		{
			Assert.ThrowsAny<JsonException>(() => Parse("{ not valid json"));
		}
	}
}
