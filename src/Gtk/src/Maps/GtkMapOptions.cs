using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps.Gtk
{
	/// <summary>
	/// Settings of the GTK map: tile servers, the identifying <c>User-Agent</c>, the disk cache and the geocoder service.
	/// Configure it with <c>UseMauiMapsGtk(options =&gt; ...)</c>.
	/// </summary>
	/// <remarks>
	/// The defaults use the public OpenStreetMap and Esri servers, whose usage policies forbid heavy or commercial use:
	/// point <see cref="StreetTiles"/>, <see cref="SatelliteTiles"/>, <see cref="HybridTiles"/> and <see cref="NominatimUrl"/> at your own
	/// servers for production. Options are read when the first map is created; change them before that.
	/// </remarks>
	public sealed class GtkMapOptions
	{
		readonly object _gate = new();
		TileStore? _store;

		/// <summary>Tiles for <c>MapType.Street</c>. Default: OpenStreetMap.</summary>
		public GtkMapTileSource StreetTiles { get; set; } = GtkMapTileSource.OpenStreetMap;

		/// <summary>Tiles for <c>MapType.Satellite</c>. Default: Esri World Imagery.</summary>
		public GtkMapTileSource SatelliteTiles { get; set; } = GtkMapTileSource.EsriSatellite;

		/// <summary>Tiles for <c>MapType.Hybrid</c>. Default: Esri World Imagery with the places overlay.</summary>
		public GtkMapTileSource HybridTiles { get; set; } = GtkMapTileSource.EsriHybrid;

		/// <summary>
		/// The <c>User-Agent</c> sent with tile and geocoding requests. The OpenStreetMap policies require one that identifies the application.
		/// Default: the entry assembly name and version plus this library.
		/// </summary>
		public string UserAgent { get; set; } = DefaultUserAgent();

		/// <summary>The directory of the tile disk cache, or <see langword="null"/> to disable it. Default: <c>$XDG_CACHE_HOME/&lt;app&gt;/map-tiles</c>.</summary>
		public string? CacheDirectory { get; set; } = DefaultCacheDirectory();

		/// <summary>How long a cached tile is used before it is downloaded again. Default: 7 days (the OpenStreetMap minimum).</summary>
		public TimeSpan CacheMaxAge { get; set; } = TimeSpan.FromDays(7);

		/// <summary>The size the disk cache is trimmed to when the first map is created. Default: 200 MB; 0 disables trimming.</summary>
		public long CacheMaxBytes { get; set; } = 200L * 1024 * 1024;

		/// <summary>How many tiles are downloaded at the same time. Default: 4 (the OpenStreetMap policy allows at most 2 connections per host for bulk, 4 for interactive).</summary>
		public int MaxConcurrentDownloads { get; set; } = 4;

		/// <summary>Replaces the HTTP downloader (for another transport, or for tests). Default: <see cref="HttpGtkMapTileFetcher"/>.</summary>
		public IGtkMapTileFetcher? TileFetcher { get; set; }

		/// <summary>The Nominatim service the geocoder queries. Default: <c>https://nominatim.openstreetmap.org</c> (at most 1 request per second).</summary>
		public string NominatimUrl { get; set; } = "https://nominatim.openstreetmap.org";

		/// <summary>
		/// Supplies the user's position for <c>Map.IsShowingUser</c>. GTK has no location service, so the app provides one (for example from its own
		/// GeoClue binding); when it returns <see langword="null"/> or is not set, no user marker is drawn.
		/// </summary>
		public Func<Location?>? UserLocationProvider { get; set; }

		/// <summary>The source for a map type.</summary>
		/// <param name="mapType">The map type.</param>
		public GtkMapTileSource GetTileSource(MapType mapType) => mapType switch
		{
			MapType.Satellite => SatelliteTiles,
			MapType.Hybrid => HybridTiles,
			_ => StreetTiles,
		};

		/// <summary>The tile store shared by every map created with these options.</summary>
		internal TileStore GetTileStore()
		{
			lock (_gate)
			{
				if (_store is not null)
					return _store;

				TileDiskCache? disk = null;
				if (!string.IsNullOrEmpty(CacheDirectory))
				{
					disk = new TileDiskCache(CacheDirectory!, CacheMaxAge);
					if (CacheMaxBytes > 0)
					{
						var cache = disk;
						var max = CacheMaxBytes;
						ThreadPool.QueueUserWorkItem(_ => cache.Trim(max));
					}
				}

				IGtkMapTileFetcher fetcher = TileFetcher ?? new HttpGtkMapTileFetcher(UserAgent);
				return _store = new TileStore(fetcher, disk, MaxConcurrentDownloads, Post);
			}
		}

		// Completion callbacks run on the GTK main loop (the map view's textures belong to it).
		static void Post(Action action) => GLib.Idle.Add(() =>
		{
			action();
			return false;
		});

		static string DefaultUserAgent()
		{
			var entry = Assembly.GetEntryAssembly()?.GetName();
			var library = typeof(GtkMapOptions).Assembly.GetName();
			var app = string.IsNullOrEmpty(entry?.Name) ? "MauiGtkApp" : entry!.Name!.Replace(' ', '_');
			var version = entry?.Version?.ToString() ?? "0.0";
			return $"{app}/{version} ({library.Name}/{library.Version})";
		}

		static string? DefaultCacheDirectory()
		{
			try
			{
				var cache = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
				if (string.IsNullOrEmpty(cache))
				{
					var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
					cache = OperatingSystem.IsWindows()
						? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
						: string.IsNullOrEmpty(home) ? null : Path.Combine(home, ".cache");
				}

				if (string.IsNullOrEmpty(cache))
					return null;

				var app = Assembly.GetEntryAssembly()?.GetName().Name ?? "MauiGtkApp";
				return Path.Combine(cache, app, "map-tiles");
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
