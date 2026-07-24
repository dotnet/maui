using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Storage;

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
	/// server, so this reconstructs the same information from the bundled manifest. The manifest is
	/// bundled outside the web root and read via the app package APIs, so it is never exposed to the
	/// web view.
	/// </summary>
	internal sealed class StaticWebAssetsManifest
	{
		/// <summary>
		/// The bundled manifest location within the app package, deliberately outside the web root
		/// (<c>wwwroot</c>) so it is never served to the web view. It is read via
		/// <see cref="FileSystem.OpenAppPackageFileAsync(string)"/>.
		/// </summary>
		internal const string ManifestPackagePath = "_maui/blazor-asset-manifest.json";

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
		/// Attempts to load and parse the bundled manifest from the app package.
		/// </summary>
		/// <returns>The parsed manifest, or <c>null</c> if it is not present or cannot be read.</returns>
		public static StaticWebAssetsManifest? TryLoad()
		{
			try
			{
				// Offload to the thread pool and block: the downstream static-content pipeline that
				// consumes this is synchronous, and the platform app-package readers complete
				// synchronously anyway. Matches how the host document is rendered.
				return Task.Run(LoadAsync).GetAwaiter().GetResult();
			}
			catch (Exception)
			{
				// A missing or malformed manifest must never break startup; fingerprinting simply stays off.
				return null;
			}
		}

		private static async Task<StaticWebAssetsManifest?> LoadAsync()
		{
			if (!await FileSystem.AppPackageFileExistsAsync(ManifestPackagePath).ConfigureAwait(false))
			{
				return null;
			}

			using var stream = await FileSystem.OpenAppPackageFileAsync(ManifestPackagePath).ConfigureAwait(false);
			return Parse(stream);
		}

		internal static StaticWebAssetsManifest Parse(Stream stream)
		{
			var data = JsonSerializer.Deserialize(stream, StaticWebAssetsManifestContext.Default.ManifestData);
			return FromData(data);
		}

		internal static StaticWebAssetsManifest FromData(ManifestData? data)
		{
			var resources = new List<ResourceAsset>();
			var seenLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var routeToPhysical = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			var endpoints = data?.Endpoints;
			if (endpoints is null || endpoints.Count == 0)
			{
				return new StaticWebAssetsManifest(ResourceAssetCollection.Empty, routeToPhysical);
			}

			foreach (var endpoint in endpoints)
			{
				var route = endpoint.Route;
				var assetFile = endpoint.AssetFile;
				if (route is null || assetFile is null)
				{
					continue;
				}

				// Endpoints with selectors are alternative representations (for example gzip/brotli
				// content negotiation). They are not distinct assets, so skip them - mirroring the
				// framework's own ResourceCollectionResolver.
				if (endpoint.Selectors is { Count: > 0 })
				{
					continue;
				}

				var isCompressed = assetFile.EndsWith(".gz", StringComparison.OrdinalIgnoreCase) ||
					assetFile.EndsWith(".br", StringComparison.OrdinalIgnoreCase);

				var (label, integrity) = ReadProperties(endpoint.EndpointProperties);

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

		private static (string? Label, string? Integrity) ReadProperties(List<EndpointProperty>? properties)
		{
			string? label = null;
			string? integrity = null;

			if (properties is not null)
			{
				foreach (var property in properties)
				{
					if (property.Name is null)
					{
						continue;
					}

					if (label is null && property.Name.Equals("label", StringComparison.OrdinalIgnoreCase))
					{
						label = property.Value;
					}
					else if (integrity is null && property.Name.Equals("integrity", StringComparison.OrdinalIgnoreCase))
					{
						integrity = property.Value;
					}
				}
			}

			return (label, integrity);
		}

		private static string NormalizePath(string path) =>
			(path ?? string.Empty).Replace('\\', '/').TrimStart('/');

		/// <summary>The subset of the endpoints manifest that hybrid asset resolution needs.</summary>
		internal sealed class ManifestData
		{
			[JsonPropertyName("Endpoints")]
			public List<Endpoint>? Endpoints { get; set; }
		}

		internal sealed class Endpoint
		{
			[JsonPropertyName("Route")]
			public string? Route { get; set; }

			[JsonPropertyName("AssetFile")]
			public string? AssetFile { get; set; }

			[JsonPropertyName("Selectors")]
			public List<EndpointSelector>? Selectors { get; set; }

			[JsonPropertyName("EndpointProperties")]
			public List<EndpointProperty>? EndpointProperties { get; set; }
		}

		internal sealed class EndpointSelector
		{
			[JsonPropertyName("Name")]
			public string? Name { get; set; }
		}

		internal sealed class EndpointProperty
		{
			[JsonPropertyName("Name")]
			public string? Name { get; set; }

			[JsonPropertyName("Value")]
			public string? Value { get; set; }
		}
	}

	[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
	[JsonSerializable(typeof(StaticWebAssetsManifest.ManifestData))]
	internal sealed partial class StaticWebAssetsManifestContext : JsonSerializerContext
	{
	}
}
