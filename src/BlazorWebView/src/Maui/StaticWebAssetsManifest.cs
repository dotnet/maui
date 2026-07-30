using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Parses the static web assets endpoints manifest (<c>*.staticwebassets.endpoints.json</c>) that
	/// is bundled with a hybrid app and exposes it as:
	/// <list type="bullet">
	/// <item><description>a <see cref="ResourceAssetCollection"/> so <c>@Assets["logical"]</c> resolves to the
	/// fingerprinted URL at render time; and</description></item>
	/// <item><description>a route-to-physical map so the web view can serve the physical asset for a
	/// fingerprinted request URL.</description></item>
	/// </list>
	/// Blazor Web Apps build this from endpoint metadata via <c>MapStaticAssets</c>; hybrid apps have no
	/// server, so this reconstructs the same information from the bundled manifest.
	/// </summary>
	internal sealed class StaticWebAssetsManifest
	{
		/// <summary>The bundled manifest location, relative to the web root (wwwroot).</summary>
		internal const string ManifestRelativePath = "_maui/asset-manifest.json";

		private StaticWebAssetsManifest(ResourceAssetCollection assets, IReadOnlyDictionary<string, string> routeToPhysicalPath)
		{
			Assets = assets;
			RouteToPhysicalPath = routeToPhysicalPath;
		}

		/// <summary>Gets the fingerprint-aware asset collection used to resolve <c>@Assets</c>.</summary>
		public ResourceAssetCollection Assets { get; }

		/// <summary>Gets the map of request route (possibly fingerprinted) to the physical asset file.</summary>
		public IReadOnlyDictionary<string, string> RouteToPhysicalPath { get; }

		/// <summary>
		/// Attempts to load and parse the bundled manifest from the given file provider.
		/// </summary>
		/// <param name="fileProvider">The platform file provider rooted at the web root.</param>
		/// <returns>The parsed manifest, or <c>null</c> if it is not present or cannot be read.</returns>
		public static StaticWebAssetsManifest? TryLoad(IFileProvider fileProvider)
		{
			if (fileProvider is null)
			{
				return null;
			}

			try
			{
				var fileInfo = fileProvider.GetFileInfo(ManifestRelativePath);
				if (fileInfo is null || !fileInfo.Exists)
				{
					return null;
				}

				using var stream = fileInfo.CreateReadStream();
				return Parse(stream);
			}
			catch (Exception)
			{
				// A missing or malformed manifest must never break startup; fingerprinting simply stays off.
				return null;
			}
		}

		internal static StaticWebAssetsManifest Parse(Stream stream)
		{
			var resources = new List<ResourceAsset>();
			var seenLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var routeToPhysical = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			using var document = JsonDocument.Parse(stream);
			if (!document.RootElement.TryGetProperty("Endpoints", out var endpoints) ||
				endpoints.ValueKind != JsonValueKind.Array)
			{
				return new StaticWebAssetsManifest(ResourceAssetCollection.Empty, routeToPhysical);
			}

			foreach (var endpoint in endpoints.EnumerateArray())
			{
				var route = GetString(endpoint, "Route");
				var assetFile = GetString(endpoint, "AssetFile");
				if (route is null || assetFile is null)
				{
					continue;
				}

				// Endpoints with selectors are alternative representations (for example gzip/brotli
				// content negotiation). They are not distinct assets, so skip them - mirroring the
				// framework's own ResourceCollectionResolver.
				var hasSelectors = endpoint.TryGetProperty("Selectors", out var selectors) &&
					selectors.ValueKind == JsonValueKind.Array &&
					selectors.GetArrayLength() > 0;
				if (hasSelectors)
				{
					continue;
				}

				var isCompressed = assetFile.EndsWith(".gz", StringComparison.OrdinalIgnoreCase) ||
					assetFile.EndsWith(".br", StringComparison.OrdinalIgnoreCase);

				var (label, integrity) = ReadProperties(endpoint);

				// @Assets resolution: map the human-readable label to the fingerprinted route. Skip
				// compressed variants and duplicate labels to avoid collisions.
				if (label is not null && !isCompressed && seenLabels.Add(label))
				{
					var properties = integrity is null
						? new[] { new ResourceAssetProperty("label", label) }
						: new[] { new ResourceAssetProperty("label", label), new ResourceAssetProperty("integrity", integrity) };
					resources.Add(new ResourceAsset(route, properties));
				}

				// Serving: map the (possibly fingerprinted) route to the physical file on disk. Prefer
				// the uncompressed asset and keep the first mapping for a given route.
				if (!isCompressed)
				{
					routeToPhysical.TryAdd(NormalizePath(route), NormalizePath(assetFile));
				}
			}

			return new StaticWebAssetsManifest(new ResourceAssetCollection(resources), routeToPhysical);
		}

		/// <summary>
		/// Resolves a requested path to the physical asset file if the request targets a fingerprinted
		/// route whose physical file has a different name.
		/// </summary>
		/// <param name="requestedPath">The web-root-relative requested path.</param>
		/// <param name="physicalPath">The physical asset path to serve, if different.</param>
		/// <returns><c>true</c> if a different physical path should be served; otherwise <c>false</c>.</returns>
		public bool TryResolvePhysicalPath(string requestedPath, out string physicalPath)
		{
			var normalized = NormalizePath(requestedPath);
			if (RouteToPhysicalPath.TryGetValue(normalized, out var mapped) &&
				!string.Equals(mapped, normalized, StringComparison.OrdinalIgnoreCase))
			{
				physicalPath = mapped;
				return true;
			}

			physicalPath = requestedPath;
			return false;
		}

		private static (string? Label, string? Integrity) ReadProperties(JsonElement endpoint)
		{
			string? label = null;
			string? integrity = null;

			if (endpoint.TryGetProperty("EndpointProperties", out var properties) &&
				properties.ValueKind == JsonValueKind.Array)
			{
				foreach (var property in properties.EnumerateArray())
				{
					var name = GetString(property, "Name");
					if (name is null)
					{
						continue;
					}

					if (label is null && name.Equals("label", StringComparison.OrdinalIgnoreCase))
					{
						label = GetString(property, "Value");
					}
					else if (integrity is null && name.Equals("integrity", StringComparison.OrdinalIgnoreCase))
					{
						integrity = GetString(property, "Value");
					}
				}
			}

			return (label, integrity);
		}

		private static string? GetString(JsonElement element, string propertyName) =>
			element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
				? value.GetString()
				: null;

		private static string NormalizePath(string path) =>
			(path ?? string.Empty).Replace('\\', '/').TrimStart('/');
	}
}
