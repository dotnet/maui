using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Loads the deterministic asset manifest that MAUI generates at build time (from the static web
	/// asset endpoint descriptors) and bundles with a hybrid app, and exposes it as:
	/// <list type="bullet">
	/// <item><description>a <see cref="ResourceAssetCollection"/> so <c>@Assets["logical"]</c> resolves to the
	/// fingerprinted URL at render time (carrying label, integrity and preload metadata); and</description></item>
	/// <item><description>a route-to-physical map so the web view can serve the physical asset for a
	/// fingerprinted request URL.</description></item>
	/// </list>
	/// Blazor Web Apps build the same <see cref="ResourceAssetCollection"/> from the endpoint descriptors via
	/// <c>MapStaticAssets</c>; hybrid apps have no server, so MAUI reconstructs it from those descriptors. The
	/// manifest is bundled outside the web root and read via the app package APIs, so it is never exposed to
	/// the web view. Its content is derived entirely from asset fingerprints, logical names and content hashes
	/// (no timestamps, absolute paths, or runtime identifiers), so it is deterministic and identical across
	/// architectures - which is required for universal (multi-RID) app bundles to merge.
	/// </summary>
	internal sealed class StaticWebAssetsManifest
	{
		/// <summary>
		/// The bundled manifest location within the app package, deliberately outside the web root
		/// (<c>wwwroot</c>) so it is never served to the web view. It is read via
		/// <see cref="FileSystem.OpenAppPackageFileAsync(string)"/>.
		/// </summary>
		internal const string ManifestPackagePath = "_maui/blazor-asset-manifest.json";

		// The manifest is immutable build output, so it is loaded and parsed once per process and the
		// result (including "not present") is cached to avoid repeatedly blocking a caller thread on
		// app-package I/O for every handler start / reconnect.
		private static StaticWebAssetsManifest? s_cached;
		private static volatile bool s_cacheLoaded;

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
		/// Attempts to load and parse the bundled manifest from the app package. The result is cached for
		/// the lifetime of the process.
		/// </summary>
		/// <param name="logger">An optional logger used to report a corrupt (but present) manifest.</param>
		/// <returns>The parsed manifest, or <c>null</c> if it is not present or cannot be read.</returns>
		public static StaticWebAssetsManifest? TryLoad(ILogger? logger = null)
		{
			if (s_cacheLoaded)
			{
				return s_cached;
			}

			StaticWebAssetsManifest? manifest = null;
			try
			{
				// Offload to the thread pool and block. This blocking overload exists only for the
				// synchronous CreateFileProvider fallback; the primary startup path awaits TryLoadAsync
				// so the UI thread is never blocked on app-package I/O.
				manifest = Task.Run(LoadAsync).GetAwaiter().GetResult();
			}
			catch (Exception ex)
			{
				LogLoadFailure(logger, ex);
			}

			return Publish(manifest);
		}

		/// <summary>
		/// Asynchronously loads and parses the bundled manifest from the app package, caching the result
		/// for the lifetime of the process. Preferred over <see cref="TryLoad"/> on any path that can
		/// await, so the (possibly genuinely asynchronous) app-package read never blocks the caller.
		/// </summary>
		/// <param name="logger">An optional logger used to report a corrupt (but present) manifest.</param>
		/// <returns>The parsed manifest, or <c>null</c> if it is not present or cannot be read.</returns>
		public static async Task<StaticWebAssetsManifest?> TryLoadAsync(ILogger? logger = null)
		{
			if (s_cacheLoaded)
			{
				return s_cached;
			}

			StaticWebAssetsManifest? manifest = null;
			try
			{
				manifest = await LoadAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				LogLoadFailure(logger, ex);
			}

			return Publish(manifest);
		}

		private static StaticWebAssetsManifest? Publish(StaticWebAssetsManifest? manifest)
		{
			// The manifest is immutable build output, so caching "not present" (null) is intentional. In
			// the rare case two starts race the first load, both produce an equivalent result.
			s_cached = manifest;
			s_cacheLoaded = true;
			return manifest;
		}

		private static void LogLoadFailure(ILogger? logger, Exception ex)
		{
			// A missing manifest returns null from LoadAsync without throwing, so this only fires for a
			// present-but-unreadable manifest. Loading an OPTIONAL fingerprinting manifest must never be
			// able to take down the WebView, so any failure is swallowed (fingerprinting stays off) and
			// logged so the resulting asset 404s are diagnosable rather than silent.
			logger?.LogWarning(ex, "Failed to load the Blazor static web assets manifest '{ManifestPath}'; asset fingerprinting is disabled.", ManifestPackagePath);
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

		// Test hook: clears the process-wide cache so a subsequent TryLoad re-reads the app package.
		internal static void ResetCacheForTests()
		{
			s_cacheLoaded = false;
			s_cached = null;
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
				var url = asset.Url;
				if (url is null)
				{
					continue;
				}

				// Rebuild the Blazor ResourceAsset from the endpoint descriptor properties (label,
				// integrity, preload*), mirroring the framework's ResourceCollectionResolver. @Assets
				// resolution uses "label"; the rest (integrity/preload) flow through for SRI and preload.
				ResourceAssetProperty[]? properties = null;
				string? label = null;
				var entryProperties = asset.Properties;
				if (entryProperties is { Count: > 0 })
				{
					var list = new List<ResourceAssetProperty>(entryProperties.Count);
					foreach (var property in entryProperties)
					{
						if (property.Name is null || property.Value is null)
						{
							continue;
						}

						list.Add(new ResourceAssetProperty(property.Name, property.Value));

						if (string.Equals(property.Name, "label", StringComparison.OrdinalIgnoreCase))
						{
							label = property.Value;
						}
					}

					if (list.Count > 0)
					{
						properties = list.ToArray();
					}
				}

				resources.Add(new ResourceAsset(url, properties));

				// Serving: a fingerprinted route maps to the physical file stored under the web root at
				// the logical label. Only recorded when the normalized route and label actually differ, so
				// the map holds no redundant identity entries (matching TryResolvePhysicalPath's contract).
				if (label is not null)
				{
					var normalizedRoute = NormalizePath(url);
					var normalizedLabel = NormalizePath(label);
					if (!string.Equals(normalizedRoute, normalizedLabel, StringComparison.OrdinalIgnoreCase))
					{
						routeToPhysical.TryAdd(normalizedRoute, normalizedLabel);
					}
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

		private static string NormalizePath(string path) =>
			(path ?? string.Empty).Replace('\\', '/').TrimStart('/');

		/// <summary>The deterministic asset manifest MAUI generates at build time from the endpoint descriptors.</summary>
		internal sealed class ManifestData
		{
			[JsonPropertyName("Assets")]
			public List<AssetEntry>? Assets { get; set; }
		}

		/// <summary>A single asset descriptor: the served URL plus the Blazor resource properties.</summary>
		internal sealed class AssetEntry
		{
			/// <summary>The served route (for example the fingerprinted <c>app.abc123.css</c>).</summary>
			[JsonPropertyName("Url")]
			public string? Url { get; set; }

			/// <summary>The endpoint properties Blazor carries into a <see cref="ResourceAsset"/> (label, integrity, preload*).</summary>
			[JsonPropertyName("Properties")]
			public List<AssetPropertyEntry>? Properties { get; set; }
		}

		/// <summary>A single <c>{ "Name": ..., "Value": ... }</c> resource property.</summary>
		internal sealed class AssetPropertyEntry
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
