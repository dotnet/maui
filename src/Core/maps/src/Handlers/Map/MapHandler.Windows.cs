using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Devices.Geolocation;

namespace Microsoft.Maui.Maps.Handlers
{
	// TODO: For .NET 11, refactor setup/cleanup into ConnectHandler/DisconnectHandler overrides
	// to match the iOS/Android handler pattern. Avoided for now to prevent new PublicAPI entries.
	/// <summary>
	/// Handler for the Map control on Windows using the WinUI 3 MapControl backed by Azure Maps.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Authentication:</b> Set your Azure Maps subscription key using:
	/// <code>builder.ConfigureEssentials(e =&gt; e.UseMapServiceToken("YOUR_AZURE_MAPS_KEY"));</code>
	/// Get a key from the Azure Portal: https://portal.azure.com → Azure Maps account → Authentication.
	/// </para>
	/// <para>
	/// <b>Supported features (via Azure Maps JS API through internal WebView2):</b>
	/// <list type="bullet">
	/// <item><description><b>MoveToRegion:</b> Navigates via <c>map.setCamera()</c>.</description></item>
	/// <item><description><b>MapType:</b> Street/Satellite/Hybrid via <c>map.setStyle()</c> (road/satellite/satellite_road_labels).</description></item>
	/// <item><description><b>IsTrafficEnabled:</b> Traffic flow and incidents via <c>map.setTraffic()</c>.</description></item>
	/// <item><description><b>IsZoomEnabled/IsScrollEnabled:</b> Independent control via <c>map.setUserInteraction()</c>.</description></item>
	/// <item><description><b>Pins:</b> Displays map pins using <see cref="MapIcon"/> on a <see cref="MapElementsLayer"/>. Pin click events are supported.</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// <b>Unsupported features (no-op on Windows):</b>
	/// <list type="bullet">
	/// <item><description><b>IsShowingUser:</b> Not built-in. Use the Geolocation API and a custom <see cref="MapIcon"/> to display user location.</description></item>
	/// <item><description><b>Polylines/Polygons/Circles:</b> <see cref="MapElementsLayer"/> only supports <see cref="MapIcon"/>. Shapes are not rendered.</description></item>
	/// <item><description><b>Pin Labels/InfoWindows:</b> <see cref="MapIcon"/> does not support labels or info windows.</description></item>
	/// <item><description><b>Map.Clicked (background):</b> The <see cref="MapControl.MapElementClick"/> event only fires for <see cref="MapElement"/> clicks, not empty map area clicks.</description></item>
	/// </list>
	/// </para>
	/// </remarks>
	public partial class MapHandler : ViewHandler<IMap, FrameworkElement>
	{
		MapControl? _mapControl;
		WebView2? _webView;
		bool _webViewReady;
		MapElementsLayer? _pinsLayer;
		MapElementsLayer? _mapElementsLayer;
		readonly List<MapIcon> _mapIcons = new();

		/// <inheritdoc/>
		protected override FrameworkElement CreatePlatformView()
		{
			_mapControl = new MapControl();

			var token = ApplicationModel.Platform.MapServiceToken;

			if (!string.IsNullOrEmpty(token))
			{
				_mapControl.MapServiceToken = token;
			}

			_mapControl.ZoomLevel = 2;
			_mapControl.InteractiveControlsVisible = true;

			_mapControl.Loaded += OnMapControlLoaded;
			_mapControl.Unloaded += OnMapControlUnloaded;

			_pinsLayer = new MapElementsLayer();
			_pinsLayer.MapElementClick += OnPinLayerElementClick;
			_mapControl.Layers.Add(_pinsLayer);

			_mapElementsLayer = new MapElementsLayer();
			_mapControl.Layers.Add(_mapElementsLayer);

			return _mapControl;
		}

		void OnMapControlLoaded(object sender, RoutedEventArgs e)
		{
			if (_mapControl == null)
				return;

			_mapControl.Loaded -= OnMapControlLoaded;

			if (!TryDiscoverWebView())
			{
				// WebView2 child may not be available yet during complex initialization.
				// Retry on LayoutUpdated until found.
				_mapControl.LayoutUpdated += OnMapControlLayoutUpdated;
			}
		}

		void OnMapControlLayoutUpdated(object? sender, object e)
		{
			if (TryDiscoverWebView() && _mapControl != null)
			{
				_mapControl.LayoutUpdated -= OnMapControlLayoutUpdated;
			}
		}

		bool TryDiscoverWebView()
		{
			if (_mapControl == null || _webView != null)
				return true;

			if (VisualTreeHelper.GetChildrenCount(_mapControl) > 0 &&
				VisualTreeHelper.GetChild(_mapControl, 0) is WebView2 webView)
			{
				_webView = webView;
				_webView.NavigationCompleted += OnWebViewNavigationCompleted;
				return true;
			}

			return false;
		}

		void OnWebViewNavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
		{
			sender.NavigationCompleted -= OnWebViewNavigationCompleted;
			_webViewReady = true;

			// Apply any pending state that was queued before the WebView was ready
			if (_pendingSpan != null)
			{
				var span = _pendingSpan;
				_pendingSpan = null;
				_ = ExecuteJsAsync(string.Format(
					CultureInfo.InvariantCulture,
					"map.setCamera({{ center: [{0}, {1}], zoom: {2}, type: 'ease', duration: 300 }});",
					span.Center.Longitude, span.Center.Latitude, CalculateZoom(span)));
			}

			// Apply initial property state via JS now that the map is ready
			if (VirtualView != null)
			{
				_ = ApplyMapTypeAsync(VirtualView.MapType);
				_ = ApplyTrafficAsync(VirtualView.IsTrafficEnabled);
				_ = ApplyUserInteractionAsync(VirtualView.IsScrollEnabled, VirtualView.IsZoomEnabled);
			}
		}

		MapSpan? _pendingSpan;

		/// <summary>
		/// Executes a JavaScript command on the Azure Maps instance inside the WebView2.
		/// </summary>
		async System.Threading.Tasks.Task ExecuteJsAsync(string script)
		{
			if (_webView == null || !_webViewReady)
				return;

			try
			{
				await _webView.ExecuteScriptAsync(script);
			}
			catch (InvalidOperationException)
			{
				// Expected: WebView2 may not be fully initialized or was disposed
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.WriteLine($"[MapHandler] JS execution failed: {ex.Message}");
			}
		}

		/// <summary>
		/// Applies the map style corresponding to the MAUI <see cref="MapType"/> via the Azure Maps JS API.
		/// </summary>
		System.Threading.Tasks.Task ApplyMapTypeAsync(MapType mapType)
		{
			// Azure Maps style names: https://learn.microsoft.com/azure/azure-maps/supported-map-styles
			var style = mapType switch
			{
				MapType.Street => "road",
				MapType.Satellite => "satellite",
				MapType.Hybrid => "satellite_road_labels",
				_ => "road"
			};
			return ExecuteJsAsync($"map.setStyle({{ style: '{style}' }});");
		}

		/// <summary>
		/// Enables or disables the traffic overlay via the Azure Maps JS API.
		/// </summary>
		System.Threading.Tasks.Task ApplyTrafficAsync(bool enabled)
		{
			if (enabled)
				return ExecuteJsAsync("map.setTraffic({ flow: 'relative', incidents: true });");
			else
				return ExecuteJsAsync("map.setTraffic({ flow: 'none', incidents: false });");
		}

		/// <summary>
		/// Configures independent scroll and zoom interaction via the Azure Maps JS API.
		/// </summary>
		System.Threading.Tasks.Task ApplyUserInteractionAsync(bool scrollEnabled, bool zoomEnabled)
		{
			var drag = scrollEnabled ? "true" : "false";
			var scroll = zoomEnabled ? "true" : "false";
			var dblClick = zoomEnabled ? "true" : "false";
			return ExecuteJsAsync($"map.setUserInteraction({{ dragPanInteraction: {drag}, scrollZoomInteraction: {scroll}, dblClickZoomInteraction: {dblClick} }});");
		}

		static double CalculateZoom(MapSpan span)
		{
			double zoomLat = Math.Log2(360.0 / span.LatitudeDegrees);
			double zoomLon = Math.Log2(360.0 / span.LongitudeDegrees);
			return Math.Clamp(Math.Min(zoomLat, zoomLon), 0, 24);
		}

		void OnMapControlUnloaded(object sender, RoutedEventArgs e)
		{
			if (_mapControl != null)
			{
				_mapControl.Loaded -= OnMapControlLoaded;
				_mapControl.LayoutUpdated -= OnMapControlLayoutUpdated;
				_mapControl.Unloaded -= OnMapControlUnloaded;

				if (_pinsLayer != null)
				{
					_pinsLayer.MapElementClick -= OnPinLayerElementClick;
					_mapControl.Layers.Remove(_pinsLayer);
					_pinsLayer = null;
				}

				if (_mapElementsLayer != null)
				{
					_mapControl.Layers.Remove(_mapElementsLayer);
					_mapElementsLayer = null;
				}
			}

			_mapIcons.Clear();
			_pendingSpan = null;
			_webView = null;
			_webViewReady = false;
		}

		void OnPinLayerElementClick(MapElementsLayer sender, MapElementClickEventArgs args)
		{
			if (args.Element is MapIcon clickedIcon)
			{
				var pin = GetPinForMapIcon(clickedIcon);
				if (pin != null)
				{
					pin.SendMarkerClick();
					return; // Pin click should not also fire map click (matches iOS/Android behavior)
				}
			}

			VirtualView?.Clicked(new Location(args.Location.Position.Latitude, args.Location.Position.Longitude));
		}

		IMapPin? GetPinForMapIcon(MapIcon mapIcon)
		{
			if (VirtualView == null)
				return null;

			for (int i = 0; i < VirtualView.Pins.Count; i++)
			{
				if (ReferenceEquals(VirtualView.Pins[i].MarkerId, mapIcon))
					return VirtualView.Pins[i];
			}
			return null;
		}

		/// <summary>
		/// Maps the <see cref="IMap.MapType"/> property via the Azure Maps JS <c>map.setStyle()</c> API.
		/// </summary>
		/// <remarks>
		/// Maps MAUI <see cref="MapType.Street"/> to <c>road</c>, <see cref="MapType.Satellite"/> to <c>satellite</c>,
		/// and <see cref="MapType.Hybrid"/> to <c>satellite_road_labels</c>.
		/// </remarks>
		public static void MapMapType(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler && mapHandler._webViewReady)
			{
				_ = mapHandler.ApplyMapTypeAsync(map.MapType);
			}
		}

		/// <summary>
		/// Maps <see cref="IMap.IsZoomEnabled"/> via the Azure Maps JS <c>map.setUserInteraction()</c> API.
		/// </summary>
		/// <remarks>
		/// Controls <c>scrollZoomInteraction</c> and <c>dblClickZoomInteraction</c> independently from scroll/drag.
		/// Also keeps <c>InteractiveControlsVisible</c> in sync so the built-in UI controls remain accessible.
		/// </remarks>
		public static void MapIsZoomEnabled(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler && mapHandler._mapControl != null)
			{
				mapHandler._mapControl.InteractiveControlsVisible = map.IsZoomEnabled || map.IsScrollEnabled;

				if (mapHandler._webViewReady)
				{
					_ = mapHandler.ApplyUserInteractionAsync(map.IsScrollEnabled, map.IsZoomEnabled);
				}
			}
		}

		/// <summary>
		/// Maps <see cref="IMap.IsScrollEnabled"/> via the Azure Maps JS <c>map.setUserInteraction()</c> API.
		/// </summary>
		/// <remarks>
		/// Controls <c>dragPanInteraction</c> independently from zoom.
		/// Also keeps <c>InteractiveControlsVisible</c> in sync so the built-in UI controls remain accessible.
		/// </remarks>
		public static void MapIsScrollEnabled(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler && mapHandler._mapControl != null)
			{
				mapHandler._mapControl.InteractiveControlsVisible = map.IsZoomEnabled || map.IsScrollEnabled;

				if (mapHandler._webViewReady)
				{
					_ = mapHandler.ApplyUserInteractionAsync(map.IsScrollEnabled, map.IsZoomEnabled);
				}
			}
		}

		/// <summary>
		/// Maps <see cref="IMap.IsTrafficEnabled"/> via the Azure Maps JS <c>map.setTraffic()</c> API.
		/// </summary>
		/// <remarks>
		/// When enabled, shows traffic flow (color-coded roads) and incident icons.
		/// When disabled, removes both traffic flow and incident overlays.
		/// </remarks>
		public static void MapIsTrafficEnabled(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler && mapHandler._webViewReady)
			{
				_ = mapHandler.ApplyTrafficAsync(map.IsTrafficEnabled);
			}
		}

		/// <summary>
		/// Maps <see cref="IMap.IsShowingUser"/>. No-op on Windows: the WinUI 3 MapControl has no built-in user location display.
		/// </summary>
		public static void MapIsShowingUser(IMapHandler handler, IMap map)
		{
			// No-op: user location requires Geolocation API + custom MapIcon.
		}

		/// <summary>
		/// Handles the <see cref="IMap.MoveToRegion"/> command by navigating via the Azure Maps JS camera API.
		/// </summary>
		/// <remarks>
		/// The WinUI 3 MapControl wraps Azure Maps in a WebView2. Setting the <c>Center</c> dependency property
		/// does not reliably navigate the map view. Instead, we call <c>map.setCamera()</c> via JavaScript.
		/// The zoom level is calculated from the <see cref="MapSpan"/> using the Spherical Mercator formula:
		/// <c>zoom = log2(360 / degrees)</c>, clamped to the Azure Maps range of 0–24.
		/// Navigation uses an <c>ease</c> animation (300ms) for smooth transitions.
		/// </remarks>
		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg)
		{
			if (arg is MapSpan mapSpan && handler is MapHandler mapHandler && mapHandler._mapControl != null)
			{
				double zoom = CalculateZoom(mapSpan);

				// Also set the DP values so they stay in sync for property reads
				mapHandler._mapControl.Center = new Geopoint(new BasicGeoposition
				{
					Latitude = mapSpan.Center.Latitude,
					Longitude = mapSpan.Center.Longitude
				});
				mapHandler._mapControl.ZoomLevel = zoom;

				// Update VisibleRegion so the virtual view tracks the current map position
				map.VisibleRegion = mapSpan;

				if (mapHandler._webViewReady)
				{
					_ = mapHandler.ExecuteJsAsync(string.Format(
						CultureInfo.InvariantCulture,
						"map.setCamera({{ center: [{0}, {1}], zoom: {2}, type: 'ease', duration: 300 }});",
						mapSpan.Center.Longitude, mapSpan.Center.Latitude, zoom));
				}
				else
				{
					// Queue it for when the WebView is ready
					mapHandler._pendingSpan = mapSpan;
				}
			}
		}

		/// <summary>
		/// Maps the <see cref="IMap.Pins"/> collection. Subscribes to collection changes for dynamic updates.
		/// </summary>
		public static void MapPins(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler)
			{
				mapHandler.UpdatePins(map.Pins);
			}
		}

		void UpdatePins(IList<IMapPin> pins)
		{
			if (_pinsLayer == null || _mapControl == null)
				return;

			// Remove each MapIcon individually to trigger proper visual refresh.
			// Layer recreation and MapElements.Clear() do not reliably update the WinUI 3 MapControl rendering.
			foreach (var icon in _mapIcons)
			{
				_pinsLayer.MapElements.Remove(icon);
			}
			_mapIcons.Clear();

			foreach (var pin in pins)
			{
				AddPinToLayer(pin);
			}
		}

		void AddPinToLayer(IMapPin pin)
		{
			if (_pinsLayer == null)
				return;

			var mapIcon = new MapIcon
			{
				Location = new Geopoint(new BasicGeoposition
				{
					Latitude = pin.Location.Latitude,
					Longitude = pin.Location.Longitude
				})
			};

			pin.MarkerId = mapIcon;
			_mapIcons.Add(mapIcon);
			_pinsLayer.MapElements.Add(mapIcon);
		}

		/// <summary>
		/// Maps the <see cref="IMap.Elements"/> collection. No-op on Windows: the WinUI 3 <see cref="MapElementsLayer"/> only supports <see cref="MapIcon"/>, not shapes.
		/// </summary>
		public static void MapElements(IMapHandler handler, IMap map)
		{
			// No-op: shapes (polylines, polygons, circles) are not supported by WinUI 3 MapElementsLayer.
		}

		/// <inheritdoc/>
		public void UpdateMapElement(IMapElement element)
		{
			// No-op: shape rendering is not supported by the WinUI 3 MapControl.
		}
	}
}
