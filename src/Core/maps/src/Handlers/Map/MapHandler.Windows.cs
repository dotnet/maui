using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Devices.Geolocation;

namespace Microsoft.Maui.Maps.Handlers
{
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
	/// <b>Supported features:</b>
	/// <list type="bullet">
	/// <item><description><b>MoveToRegion:</b> Sets the map center and zoom level from the <see cref="MapSpan"/>.</description></item>
	/// <item><description><b>Pins:</b> Displays map pins using <see cref="MapIcon"/> on a <see cref="MapElementsLayer"/>. Pin click events are supported.</description></item>
	/// <item><description><b>InteractiveControls:</b> <see cref="IMap.IsZoomEnabled"/> and <see cref="IMap.IsScrollEnabled"/> control the <c>InteractiveControlsVisible</c> property (zoom, rotate, pitch, style picker are bundled together).</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// <b>Unsupported features (no-op on Windows):</b>
	/// <list type="bullet">
	/// <item><description><b>MapType:</b> The WinUI 3 MapControl does not support programmatic map style switching. Users can change styles via the built-in picker when <c>InteractiveControlsVisible</c> is enabled.</description></item>
	/// <item><description><b>IsTrafficEnabled:</b> Not supported by the WinUI 3 MapControl.</description></item>
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
		MapElementsLayer? _pinsLayer;
		MapElementsLayer? _mapElementsLayer;
		readonly List<MapIcon> _mapIcons = new();
		INotifyCollectionChanged? _pinsCollectionObservable;

		/// <inheritdoc/>
		protected override FrameworkElement CreatePlatformView()
		{
			_mapControl = new MapControl();

			var token = ApplicationModel.Platform.MapServiceToken;
			if (!string.IsNullOrEmpty(token))
			{
				_mapControl.MapServiceToken = token;
			}

			_pinsLayer = new MapElementsLayer();
			_pinsLayer.MapElementClick += OnPinLayerElementClick;
			_mapControl.Layers.Add(_pinsLayer);

			_mapElementsLayer = new MapElementsLayer();
			_mapControl.Layers.Add(_mapElementsLayer);

			_mapControl.Unloaded += OnMapControlUnloaded;

			return _mapControl;
		}

		void OnMapControlUnloaded(object sender, RoutedEventArgs e)
		{
			if (_mapControl != null)
			{
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

			if (_pinsCollectionObservable != null)
			{
				_pinsCollectionObservable.CollectionChanged -= OnPinsCollectionChanged;
				_pinsCollectionObservable = null;
			}

			_mapIcons.Clear();
			_mapControl = null;
		}

		void OnPinLayerElementClick(MapElementsLayer sender, MapElementClickEventArgs args)
		{
			if (args.Element is MapIcon clickedIcon)
			{
				var pin = GetPinForMapIcon(clickedIcon);
				pin?.SendMarkerClick();
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
		/// Maps the <see cref="IMap.MapType"/> property. No-op on Windows: the WinUI 3 MapControl does not support programmatic style changes.
		/// </summary>
		public static void MapMapType(IMapHandler handler, IMap map)
		{
			// No-op: Azure Maps style is user-controlled via InteractiveControlsVisible picker.
		}

		/// <summary>
		/// Maps <see cref="IMap.IsZoomEnabled"/>. Controls the <c>InteractiveControlsVisible</c> property together with <see cref="IMap.IsScrollEnabled"/>.
		/// </summary>
		public static void MapIsZoomEnabled(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler && mapHandler._mapControl != null)
			{
				mapHandler._mapControl.InteractiveControlsVisible = map.IsZoomEnabled || map.IsScrollEnabled;
			}
		}

		/// <summary>
		/// Maps <see cref="IMap.IsScrollEnabled"/>. Controls the <c>InteractiveControlsVisible</c> property together with <see cref="IMap.IsZoomEnabled"/>.
		/// </summary>
		public static void MapIsScrollEnabled(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler && mapHandler._mapControl != null)
			{
				mapHandler._mapControl.InteractiveControlsVisible = map.IsZoomEnabled || map.IsScrollEnabled;
			}
		}

		/// <summary>
		/// Maps <see cref="IMap.IsTrafficEnabled"/>. No-op on Windows: the WinUI 3 MapControl does not expose a traffic layer.
		/// </summary>
		public static void MapIsTrafficEnabled(IMapHandler handler, IMap map)
		{
			// No-op: traffic is not available on the WinUI 3 MapControl.
		}

		/// <summary>
		/// Maps <see cref="IMap.IsShowingUser"/>. No-op on Windows: the WinUI 3 MapControl has no built-in user location display.
		/// </summary>
		public static void MapIsShowingUser(IMapHandler handler, IMap map)
		{
			// No-op: user location requires Geolocation API + custom MapIcon.
		}

		/// <summary>
		/// Handles the <see cref="IMap.MoveToRegion"/> command by setting the map center and zoom level.
		/// </summary>
		/// <remarks>
		/// The zoom level is calculated from the <see cref="MapSpan"/> using the Spherical Mercator formula:
		/// <c>zoom = log2(360 / degrees)</c>, clamped to the Azure Maps range of 0–24.
		/// The minimum of latitude-based and longitude-based zoom is used to ensure the full span is visible.
		/// </remarks>
		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg)
		{
			if (arg is MapSpan mapSpan && handler is MapHandler mapHandler && mapHandler._mapControl != null)
			{
				var center = new Geopoint(new BasicGeoposition
				{
					Latitude = mapSpan.Center.Latitude,
					Longitude = mapSpan.Center.Longitude
				});

				mapHandler._mapControl.Center = center;

				// Calculate zoom from span using Spherical Mercator: zoom = log2(360 / degrees)
				// Use the minimum of lat and lon zoom to ensure the full span is visible
				double zoomLat = Math.Log2(360.0 / mapSpan.LatitudeDegrees);
				double zoomLon = Math.Log2(360.0 / mapSpan.LongitudeDegrees);
				double zoom = Math.Min(zoomLat, zoomLon);

				// Clamp to Azure Maps range (0-24)
				mapHandler._mapControl.ZoomLevel = Math.Clamp(zoom, 0, 24);
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
			if (_pinsLayer == null)
				return;

			// Unsubscribe from previous collection
			if (_pinsCollectionObservable != null)
			{
				_pinsCollectionObservable.CollectionChanged -= OnPinsCollectionChanged;
				_pinsCollectionObservable = null;
			}

			_pinsLayer.MapElements.Clear();
			_mapIcons.Clear();

			foreach (var pin in pins)
			{
				AddPinToLayer(pin);
			}

			// Subscribe to collection changes
			if (pins is INotifyCollectionChanged newObservable)
			{
				newObservable.CollectionChanged += OnPinsCollectionChanged;
				_pinsCollectionObservable = newObservable;
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

		void RemovePinFromLayer(IMapPin pin)
		{
			if (_pinsLayer == null || pin.MarkerId is not MapIcon mapIcon)
				return;

			_pinsLayer.MapElements.Remove(mapIcon);
			_mapIcons.Remove(mapIcon);
			pin.MarkerId = null;
		}

		void OnPinsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewItems != null)
					{
						foreach (IMapPin pin in e.NewItems)
							AddPinToLayer(pin);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					if (e.OldItems != null)
					{
						foreach (IMapPin pin in e.OldItems)
							RemovePinFromLayer(pin);
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					if (VirtualView != null)
						UpdatePins(VirtualView.Pins);
					break;
			}
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
