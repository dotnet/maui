# Microsoft.Maui.Controls.Maps.Gtk

The MAUI `Map` control on GTK 4: an OpenStreetMap-compatible slippy-tile map in a custom widget (`MauiGtkMapView`). No native map
library is needed. Tiles are decoded to `Gdk.Texture` and appended to the GTK snapshot; pins, shapes, info window and attribution are
drawn with Cairo (`Snapshot.AppendCairo`).

## Use

```csharp
builder.UseMauiApp<App>()
       .UseMauiGtk()                 // or UseMauiGtkCore()
       .UseMauiMapsGtk(options =>    // registers Controls.Maps (UseMauiMaps) + the GTK handler + Nominatim IGeocoding
       {
           options.UserAgent = "MyApp/1.0 (me@example.com)";               // required by the OSM policies
           options.StreetTiles = new GtkMapTileSource("https://tiles.example/{z}/{x}/{y}.png", "© Me", maxZoom: 18);
       });
```

The call works before or after `UseMauiGtk()` (it re-applies `GtkHandlerMap` after the map registrations).

## Options (`GtkMapOptions`)

| Option | Default | Notes |
|---|---|---|
| `StreetTiles` / `SatelliteTiles` / `HybridTiles` | OpenStreetMap / Esri World Imagery / Esri + places overlay | `GtkMapTileSource(urlTemplate, attribution, maxZoom, overlay)`; `{z}`, `{x}`, `{y}`. The public servers forbid heavy or commercial use: use your own for production |
| `UserAgent` | `<entry assembly>/<version> (Microsoft.Maui.Controls.Maps.Gtk/<version>)` | sent with tile and geocoding requests |
| `CacheDirectory` | `$XDG_CACHE_HOME/<app>/map-tiles` (`~/.cache/...`) | `null` disables the disk cache |
| `CacheMaxAge` / `CacheMaxBytes` | 7 days / 200 MB | older entries are re-downloaded; the cache is trimmed (oldest first) when the first map is created |
| `MaxConcurrentDownloads` | 4 | newest request first, so panned-past tiles do not delay the visible ones |
| `TileFetcher` | `HttpGtkMapTileFetcher` | `IGtkMapTileFetcher`: another transport, or a stub in tests |
| `NominatimUrl` | `https://nominatim.openstreetmap.org` | geocoder service; requests are spaced 1 s apart |
| `UserLocationProvider` | none | position for `IsShowingUser` (GTK has no location service) |

## Supported

| `Map` feature | GTK |
|---|---|
| `MapType` | street / satellite / hybrid tile sources |
| `IsScrollEnabled`, `IsZoomEnabled` | drag pans; wheel zooms about the pointer; double click zooms in |
| `Pins` | teardrop marker coloured by `PinType`; click raises `MarkerClicked` and opens an info window (label + address) unless the handler sets `HideInfoWindow`; clicking the info window raises `InfoWindowClicked` |
| `MapElements` | `Polyline`, `Polygon` (fill + stroke), `Circle` (radius in metres); property changes repaint |
| `MoveToRegion`, `VisibleRegion` | fits the span at the largest zoom that contains it; `VisibleRegion` follows pans and zooms (coalesced on the idle loop) |
| `MapClicked` | empty-map click, with the clicked `Location` |
| `IsShowingUser` | blue dot at `UserLocationProvider()` |
| `IsTrafficEnabled` | ignored (no free traffic tiles) |
| `Geocoding` | `NominatimGeocoding : IGeocoding`, registered unless the app already registered one |

Not supported: stroke cap/join/dash (the Controls map elements do not implement them), custom pin icons, rotation/tilt, pinch zoom,
HiDPI-sharp tiles (tiles are 256 logical px and scaled by GTK), offline regions.

## Layout

| File | Role |
|---|---|
| `Tiles/MercatorProjection.cs`, `Tiles/MapViewport.cs` | pure Web-Mercator math, pixel <-> position, visible region |
| `Tiles/TileStore.cs`, `TileDiskCache.cs`, `GtkMapTileFetcher.cs` | download queue (bounded, newest first, de-duplicated, 30 s failure back-off), atomic disk cache, HTTP fetcher |
| `MauiGtkMapView*.cs` | the widget: input (gestures), snapshot (textures + Cairo overlay), hit testing |
| `GtkMapHandler.cs` | `MapHandler` subclass; mappers via `GtkMapperBuilder` (the stock Standard map mappings throw, so they are not chained) |
| `NominatimGeocoding.cs` | `IGeocoding` over Nominatim (`System.Text.Json`) |

## Tests

`src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests/Maps` (83 tests): projection and viewport math, tile source and options, LRU, disk cache, tile store
(dedupe, concurrency, ordering, failures), HTTP fetcher and geocoder against stub handlers, handler mappers, input, and pixel tests on a real
window with a stub tile server (tile colour, placeholders, pins, polygon/polyline/circle, user marker, attribution). No test uses the network.
