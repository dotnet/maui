using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
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

		// Builds a manifest entry in the descriptor schema: { "Url": url, "Properties": [ {Name,Value}, ... ] }.
		static string Entry(string? url, params (string Name, string Value)[] properties)
		{
			static string Esc(string v) => v.Replace("\\", "\\\\", System.StringComparison.Ordinal).Replace("\"", "\\\"", System.StringComparison.Ordinal);
			var props = string.Join(",", properties.Select(p =>
				p.Value is null
					? $"{{\"Name\":\"{p.Name}\",\"Value\":null}}"
					: $"{{\"Name\":\"{Esc(p.Name)}\",\"Value\":\"{Esc(p.Value)}\"}}"));
			return url is null
				? $"{{\"Url\":null,\"Properties\":[{props}]}}"
				: $"{{\"Url\":\"{Esc(url)}\",\"Properties\":[{props}]}}";
		}

		static string Manifest(params string[] entries) => "{\"Assets\":[" + string.Join(",", entries) + "]}";

		[Fact]
		public void EmptyAssetsArrayProducesEmptyMap()
		{
			var manifest = Parse("{\"Assets\":[]}");

			Assert.Empty(manifest.RouteToPhysicalPath);
			Assert.Empty(manifest.Assets);
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
			var manifest = Parse(Manifest(Entry("app.abc123.css", ("label", "app.css"))));

			Assert.True(manifest.TryResolvePhysicalPath("app.abc123.css", out var physical));
			Assert.Equal("app.css", physical);
		}

		[Fact]
		public void ResourceAssetCollectionExposesUrlLabelAndIntegrity()
		{
			var manifest = Parse(Manifest(Entry("app.abc123.css",
				("label", "app.css"),
				("integrity", "sha256-abc"))));

			// @Assets["app.css"] resolves to the fingerprinted URL...
			Assert.Equal("app.abc123.css", manifest.Assets["app.css"]);

			// ...and the descriptor carries the integrity property through, like ResourceCollectionResolver.
			var asset = Assert.Single(manifest.Assets);
			Assert.Equal("app.abc123.css", asset.Url);
			Assert.Contains(asset.Properties!, p => p.Name == "label" && p.Value == "app.css");
			Assert.Contains(asset.Properties!, p => p.Name == "integrity" && p.Value == "sha256-abc");
		}

		[Fact]
		public void PreloadPropertiesArePreserved()
		{
			var manifest = Parse(Manifest(Entry("lib.abc.js",
				("label", "lib.js"),
				("integrity", "sha256-xyz"),
				("preloadrel", "modulepreload"),
				("preloadgroup", "js"))));

			var asset = Assert.Single(manifest.Assets);
			Assert.Contains(asset.Properties!, p => p.Name == "preloadrel" && p.Value == "modulepreload");
			Assert.Contains(asset.Properties!, p => p.Name == "preloadgroup" && p.Value == "js");
		}

		[Fact]
		public void EntryWithoutLabelIsNotAddedToServingMapButStaysInCollection()
		{
			// A non-fingerprinted endpoint (integrity only, no label) is a real ResourceAsset, but there
			// is no fingerprinted route to remap for serving.
			var manifest = Parse(Manifest(Entry("plain.css", ("integrity", "sha256-plain"))));

			Assert.Empty(manifest.RouteToPhysicalPath);
			var asset = Assert.Single(manifest.Assets);
			Assert.Equal("plain.css", asset.Url);
			Assert.Contains(asset.Properties!, p => p.Name == "integrity");
		}

		[Fact]
		public void EntriesWithNullUrlAreSkipped()
		{
			var manifest = Parse(Manifest(
				Entry(null, ("label", "a.css")),
				Entry("c.abc.css", ("label", "c.css"))));

			Assert.Single(manifest.RouteToPhysicalPath);
			Assert.True(manifest.TryResolvePhysicalPath("c.abc.css", out var physical));
			Assert.Equal("c.css", physical);
		}

		[Fact]
		public void DuplicateRoutesResolveFirstWins()
		{
			var manifest = Parse(Manifest(
				Entry("dup.css", ("label", "first.css")),
				Entry("dup.css", ("label", "second.css"))));

			Assert.True(manifest.TryResolvePhysicalPath("dup.css", out var physical));
			Assert.Equal("first.css", physical);
		}

		[Fact]
		public void RouteEqualToLabelDoesNotResolve()
		{
			var manifest = Parse(Manifest(Entry("same.css", ("label", "same.css"))));

			Assert.False(manifest.TryResolvePhysicalPath("same.css", out _));
		}

		[Fact]
		public void LeadingSlashAndBackslashAreNormalized()
		{
			var manifest = Parse(Manifest(Entry("/sub\\x.abc.css", ("label", "/sub/x.css"))));

			Assert.True(manifest.TryResolvePhysicalPath("sub/x.abc.css", out var physical));
			Assert.Equal("sub/x.css", physical);
		}

		[Fact]
		public void UnknownRouteDoesNotResolve()
		{
			var manifest = Parse(Manifest(Entry("app.abc123.css", ("label", "app.css"))));

			Assert.False(manifest.TryResolvePhysicalPath("other.css", out _));
		}

		[Fact]
		public void MalformedJsonThrows()
		{
			Assert.ThrowsAny<JsonException>(() => Parse("{ not valid json"));
		}
	}
}
