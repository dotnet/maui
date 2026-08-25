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
	/// Loads the minimal fingerprinted-asset manifest that MAUI generates at build time (from the
	/// static web assets endpoints) and bundles with a hybrid app, and exposes it as:
	/// <list type="bullet">
	/// <item><description>a <see cref="ResourceAssetCollection"/> so <c>@Assets["logical"]</c> resolves to the
	/// fingerprinted URL at render time; and</description></item>
	/// <item><description>a route-to-physical map so the web view can serve the physical asset for a
	/// fingerprinted request URL.</description></item>
	/// </list>
	/// Blazor Web Apps build this from endpoint metadata via <c>MapStaticAssets</c>; hybrid apps have no
	/// server, so MAUI reconstructs just the fingerprint mapping at build time. The manifest is bundled
	/// outside the web root and read via the app package APIs, so it is never exposed to the web view.
	/// Its content is derived entirely from asset fingerprints and logical names (no timestamps, absolute
	/// paths, or runtime identifiers), so it is deterministic and identical across architectures - which
	/// is required for universal (multi-RID) app bundles to merge.
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

		/// <summary>Gets the map of fingerprinted request route to the physical asset file under the web root.</summary>
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
			var routeToPhysical = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			var assets = data?.Assets;
			if (assets is null || assets.Count == 0)
			{
				return new StaticWebAssetsManifest(ResourceAssetCollection.Empty, routeToPhysical);
			}

			foreach (var asset in assets)
			{
				var route = asset.Route;
				var label = asset.Label;
				if (route is null || label is null)
				{
					continue;
				}

				// @Assets resolution: the fingerprinted route is exposed under its logical label.
				resources.Add(new ResourceAsset(route, new[] { new ResourceAssetProperty("label", label) }));

				// Serving: the physical file under the web root is the logical (non-fingerprinted) label.
				routeToPhysical.TryAdd(NormalizePath(route), NormalizePath(label));
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

		private static string NormalizePath(string path) =>
			(path ?? string.Empty).Replace('\\', '/').TrimStart('/');

		/// <summary>The minimal fingerprint manifest MAUI generates at build time.</summary>
		internal sealed class ManifestData
		{
			[JsonPropertyName("Assets")]
			public List<AssetEntry>? Assets { get; set; }
		}

		internal sealed class AssetEntry
		{
			/// <summary>The fingerprinted, served route (for example <c>_content/Pkg/app.abc123.css</c>).</summary>
			[JsonPropertyName("Route")]
			public string? Route { get; set; }

			/// <summary>The logical, non-fingerprinted path used by <c>@Assets</c> and stored on disk under the web root.</summary>
			[JsonPropertyName("Label")]
			public string? Label { get; set; }
		}
	}

	[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
	[JsonSerializable(typeof(StaticWebAssetsManifest.ManifestData))]
	internal sealed partial class StaticWebAssetsManifestContext : JsonSerializerContext
	{
	}
}
