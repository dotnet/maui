#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Maps.Platform.MauiMKMapView;
#elif MONOANDROID
using PlatformView = Android.Gms.Maps.MapView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	/// <summary>
	/// Handler for the <see cref="IMap"/> control that manages the platform-specific map implementation.
	/// </summary>
	public partial class MapHandler : IMapHandler
	{
		/// <summary>
		/// The property mapper that maps cross-platform properties to platform-specific methods.
		/// </summary>
		public static IPropertyMapper<IMap, IMapHandler> Mapper = new PropertyMapper<IMap, IMapHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IMap.MapType)] = MapMapType,
			[nameof(IMap.IsShowingUser)] = MapIsShowingUser,
			[nameof(IMap.IsScrollEnabled)] = MapIsScrollEnabled,
			[nameof(IMap.IsTrafficEnabled)] = MapIsTrafficEnabled,
			[nameof(IMap.IsZoomEnabled)] = MapIsZoomEnabled,
			[nameof(IMap.IsClusteringEnabled)] = MapIsClusteringEnabled,

			[nameof(IMap.MapStyle)] = MapMapStyle,
			[nameof(IMap.Pins)] = MapPins,
			[nameof(IMap.Elements)] = MapElements,
		};

		/// <summary>
		/// The command mapper that maps cross-platform commands to platform-specific methods.
		/// </summary>
		public static CommandMapper<IMap, IMapHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IMap.MoveToRegion)] = MapMoveToRegion,
			[nameof(IMap.ShowInfoWindow)] = MapShowInfoWindow,
			[nameof(IMap.HideInfoWindow)] = MapHideInfoWindow,
			[nameof(IMapHandler.UpdateMapElement)] = MapUpdateMapElement,
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="MapHandler"/> class with default mappers.
		/// </summary>
		public MapHandler() : base(Mapper, CommandMapper)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MapHandler"/> class with optional custom mappers.
		/// </summary>
		/// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
		/// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
		public MapHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IMap IMapHandler.VirtualView => VirtualView;

		PlatformView IMapHandler.PlatformView => PlatformView;

		/// <summary>
		/// Maps the <see cref="IMapHandler.UpdateMapElement"/> command to the platform-specific implementation.
		/// </summary>
		/// <param name="handler">The map handler.</param>
		/// <param name="map">The map control.</param>
		/// <param name="arg">The <see cref="MapElementHandlerUpdate"/> argument.</param>
		public static void MapUpdateMapElement(IMapHandler handler, IMap map, object? arg)
		{
			if (arg is not MapElementHandlerUpdate args)
				return;

			handler.UpdateMapElement(args.MapElement);
		}

		/// <summary>
		/// Maps the <see cref="IMap.ShowInfoWindow"/> command to the platform-specific implementation.
		/// </summary>
		public static void MapShowInfoWindow(IMapHandler handler, IMap map, object? arg)
		{
			if (arg is IMapPin pin && handler is MapHandler mapHandler)
				mapHandler.ShowInfoWindow(pin);
		}

		/// <summary>
		/// Maps the <see cref="IMap.HideInfoWindow"/> command to the platform-specific implementation.
		/// </summary>
		public static void MapHideInfoWindow(IMapHandler handler, IMap map, object? arg)
		{
			if (arg is IMapPin pin && handler is MapHandler mapHandler)
				mapHandler.HideInfoWindow(pin);
		}

		// Builds a stable cache key for a cluster icon so logically identical images (same file, URI,
		// or font glyph) share one decoded/rasterized bitmap even when the provider hands back a fresh
		// ImageSource instance on every recluster. Returns null for sources that can't be keyed stably
		// (e.g. streams), so those are simply loaded fresh instead of being cached forever.
		// A URI source with CachingEnabled == false has explicitly opted out of caching, so it must not
		// be frozen by this handler-level cache either.
		internal static string? GetClusterIconCacheKey(IImageSource? source) =>
			source switch
			{
				IFileImageSource file when !string.IsNullOrEmpty(file.File) => $"file:{file.File}",
				// A source that opted out of caching must not be frozen by the handler-level cache either.
				IUriImageSource uri when uri.Uri is not null && uri.CachingEnabled && uri.CacheValidity > TimeSpan.Zero => $"uri:{uri.Uri}",
				IFontImageSource font when !string.IsNullOrEmpty(font.Glyph) =>
					$"font:{font.Glyph}|{font.Font.Family}|{font.Font.Size}|{font.Font.Weight}|{font.Font.Slant}|{font.Font.AutoScalingEnabled}|{font.Color?.ToArgbHex()}",
				_ => null,
			};

		// URI sources carry an explicit CacheValidity; other stable sources never expire on their own
		// (the cache is bounded and cleared during handler cleanup). Clamped so a large
		// validity like TimeSpan.MaxValue ("cache forever") can't overflow DateTime arithmetic.
		internal static DateTime GetClusterIconCacheExpiry(IImageSource? source)
		{
			if (source is not IUriImageSource uri)
				return DateTime.MaxValue;

			var now = DateTime.UtcNow;
			return uri.CacheValidity < DateTime.MaxValue - now ? now + uri.CacheValidity : DateTime.MaxValue;
		}
	}

	internal sealed class ClusterIconCache<T>
		where T : class
	{
		readonly Dictionary<string, (T Value, DateTime ExpiresAtUtc, long AccessTick)> _entries = new();
		readonly Dictionary<string, Lazy<Task<T?>>> _inFlight = new();
		readonly object _sync = new();
		readonly int _capacity;
		readonly Action<T>? _disposeValue;
		long _accessCounter;
		int _generation;

		internal ClusterIconCache(int capacity, Action<T>? disposeValue = null)
		{
			if (capacity <= 0)
				throw new ArgumentOutOfRangeException(nameof(capacity));

			_capacity = capacity;
			_disposeValue = disposeValue;
		}

		internal int Count
		{
			get
			{
				lock (_sync)
					return _entries.Count;
			}
		}

		internal bool TryGet(string? key, out T? value)
		{
			value = null;
			if (key is null)
				return false;

			lock (_sync)
				return TryGetCore(key, DateTime.UtcNow, out value);
		}

		internal async Task<T?> GetOrCreateAsync(string? key, Func<Task<T?>> valueFactory, Func<DateTime> expiryFactory)
		{
			if (valueFactory is null)
				throw new ArgumentNullException(nameof(valueFactory));
			if (expiryFactory is null)
				throw new ArgumentNullException(nameof(expiryFactory));

			if (key is null)
				return await valueFactory();

			Lazy<Task<T?>> pending;
			lock (_sync)
			{
				if (TryGetCore(key, DateTime.UtcNow, out var cached))
					return cached;

				if (_inFlight.TryGetValue(key, out var existing) && existing is not null)
				{
					pending = existing;
				}
				else
				{
					var generation = _generation;
					pending = new Lazy<Task<T?>>(
						() => CreateAndCacheAsync(key, generation, valueFactory, expiryFactory),
						LazyThreadSafetyMode.ExecutionAndPublication);
					_inFlight[key] = pending;
				}
			}

			try
			{
				return await pending.Value;
			}
			finally
			{
				lock (_sync)
				{
					if (_inFlight.TryGetValue(key, out var current) &&
						ReferenceEquals(current, pending))
					{
						_inFlight.Remove(key);
					}
				}
			}
		}

		async Task<T?> CreateAndCacheAsync(string key, int generation, Func<Task<T?>> valueFactory, Func<DateTime> expiryFactory)
		{
			var value = await valueFactory();
			if (value is null)
				return null;

			var expiresAtUtc = expiryFactory();
			lock (_sync)
			{
				if (generation != _generation)
				{
					_disposeValue?.Invoke(value);
					return null;
				}

				if (TryGetCore(key, DateTime.UtcNow, out var cached))
				{
					if (!ReferenceEquals(value, cached))
						_disposeValue?.Invoke(value);
					return cached;
				}

				if (_entries.Count >= _capacity)
					EvictLeastRecentlyUsed();

				_entries[key] = (value, expiresAtUtc, unchecked(++_accessCounter));
			}

			return value;
		}

		internal void Clear()
		{
			lock (_sync)
			{
				foreach (var entry in _entries.Values)
					_disposeValue?.Invoke(entry.Value);

				_entries.Clear();
				_inFlight.Clear();
				_accessCounter = 0;
				unchecked
				{ _generation++; }
			}
		}

		bool TryGetCore(string key, DateTime now, out T? value)
		{
			if (!_entries.TryGetValue(key, out var entry))
			{
				value = null;
				return false;
			}

			if (now >= entry.ExpiresAtUtc)
			{
				_entries.Remove(key);
				_disposeValue?.Invoke(entry.Value);
				value = null;
				return false;
			}

			value = entry.Value;
			_entries[key] = (entry.Value, entry.ExpiresAtUtc, unchecked(++_accessCounter));
			return true;
		}

		void EvictLeastRecentlyUsed()
		{
			string? oldestKey = null;
			long oldestAccess = long.MaxValue;

			foreach (var entry in _entries)
			{
				if (entry.Value.AccessTick < oldestAccess)
				{
					oldestAccess = entry.Value.AccessTick;
					oldestKey = entry.Key;
				}
			}

			if (oldestKey is not null)
			{
				var value = _entries[oldestKey].Value;
				_entries.Remove(oldestKey);
				_disposeValue?.Invoke(value);
			}
		}
	}
}
