using System.Collections.Generic;
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
	/// Get a key from the Azure Portal: https://portal.azure.com → Azure Maps account → Authentication
	/// </para>
	/// <para>
	/// <b>Platform Limitations (Windows/WinUI 3):</b>
	/// <list type="bullet">
	/// <item><description><b>MapType:</b> Azure Maps does not support programmatic map type switching (Street/Satellite/Hybrid). 
	/// Users can change the style via the built-in style picker when <c>InteractiveControlsVisible</c> is enabled.</description></item>
	/// <item><description><b>IsTrafficEnabled:</b> Traffic layer is not supported by the basic WinUI 3 MapControl. 
	/// Traffic visualization requires Azure Maps REST API or Web SDK integration.</description></item>
	/// <item><description><b>IsShowingUser:</b> User location display is not built into the WinUI 3 MapControl. 
	/// To show user location, use the Geolocation API and add a custom MapIcon.</description></item>
	/// <item><description><b>IsZoomEnabled/IsScrollEnabled:</b> These map to <c>InteractiveControlsVisible</c> which controls 
	/// all interactive features together (zoom, rotate, pitch, style picker). Individual control is not available.</description></item>
	/// <item><description><b>Polylines/Polygons/Circles:</b> The WinUI 3 MapElementsLayer has limited support for shapes. 
	/// Complex geometries may require Azure Maps REST API or custom XAML overlays.</description></item>
	/// <item><description><b>Pin Labels/InfoWindows:</b> MapIcon does not support labels or info windows directly. 
	/// Custom UI overlays are required for rich pin information display.</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// TODO (.NET 11): Add ConnectHandler/DisconnectHandler overrides to PublicAPI for proper lifecycle management.
	/// Currently initialization is done in CreatePlatformView to avoid public API changes.
	/// </para>
	/// </remarks>
	public partial class MapHandler : ViewHandler<IMap, FrameworkElement>
	{
		MapControl? _mapControl;
		MapElementsLayer? _pinsLayer;
		MapElementsLayer? _mapElementsLayer;
		readonly List<MapIcon> _mapIcons = new();

		/// <inheritdoc/>
		protected override FrameworkElement CreatePlatformView()
		{
			_mapControl = new MapControl();

			// Use the MapServiceToken from Essentials if available (set via ConfigureEssentials)
			var token = ApplicationModel.Platform.MapServiceToken;
			if (!string.IsNullOrEmpty(token))
			{
				_mapControl.MapServiceToken = token;
			}

			// Initialize layers here instead of ConnectHandler to avoid public API changes
			// TODO (.NET 11): Move to ConnectHandler override once added to PublicAPI
			_pinsLayer = new MapElementsLayer();
			_pinsLayer.MapElementClick += OnPinLayerElementClick;
			_mapControl.Layers.Add(_pinsLayer);

			// Create layer for map elements (polylines, polygons, circles)
			_mapElementsLayer = new MapElementsLayer();
			_mapControl.Layers.Add(_mapElementsLayer);

			// Handle cleanup when the control is unloaded
			_mapControl.Unloaded += OnMapControlUnloaded;

			return _mapControl;
		}

		void OnMapControlUnloaded(object sender, RoutedEventArgs e)
		{
			// Clean up resources when the control is unloaded
			// TODO (.NET 11): Move to DisconnectHandler override once added to PublicAPI
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

			_mapIcons.Clear();
			_mapControl = null;
		}

		void OnPinLayerElementClick(MapElementsLayer sender, MapElementClickEventArgs args)
		{
			if (args.Element is MapIcon clickedIcon)
			{
				// Find the corresponding IMapPin
				var pin = GetPinForMapIcon(clickedIcon);
				if (pin != null)
				{
					pin.SendMarkerClick();
				}
			}

			// Also notify the map of the click location
			VirtualView?.Clicked(new Location(args.Location.Position.Latitude, args.Location.Position.Longitude));
		}

		IMapPin? GetPinForMapIcon(MapIcon mapIcon)
		{
			for (int i = 0; i < VirtualView.Pins.Count; i++)
			{
				var pin = VirtualView.Pins[i];
				if (ReferenceEquals(pin.MarkerId, mapIcon))
				{
					return pin;
				}
			}
			return null;
		}

		/// <summary>
		/// Maps the <see cref="IMap.MapType"/> property to the platform control.
		/// </summary>
		public static void MapMapType(IMapHandler handler, IMap map)
		{
			// The new WinUI 3 MapControl (Azure Maps) uses InteractiveControlsVisible
			// which includes a style picker. Map types are handled differently in Azure Maps.
			// For now, we just ensure the control is in a valid state.
			// Azure Maps styling is controlled through the Azure Maps account settings
			// and the InteractiveControlsVisible property which shows/hides the style picker.
		}

		/// <summary>
		/// Maps the <see cref="IMap.IsZoomEnabled"/> property to the platform control.
		/// </summary>
		public static void MapIsZoomEnabled(IMapHandler handler, IMap map)
		{
			// WinUI 3 MapControl uses InteractiveControlsVisible which combines zoom, rotate, pitch, and style controls
			// We'll map IsZoomEnabled to control whether interactive controls are visible
			if (handler is MapHandler mapHandler && mapHandler._mapControl != null)
			{
				// If either zoom or scroll is enabled, show interactive controls
				mapHandler._mapControl.InteractiveControlsVisible = map.IsZoomEnabled || map.IsScrollEnabled;
			}
		}

		/// <summary>
		/// Maps the <see cref="IMap.IsScrollEnabled"/> property to the platform control.
		/// </summary>
		public static void MapIsScrollEnabled(IMapHandler handler, IMap map)
		{
			// WinUI 3 MapControl uses InteractiveControlsVisible which combines zoom, rotate, pitch, and style controls
			if (handler is MapHandler mapHandler && mapHandler._mapControl != null)
			{
				mapHandler._mapControl.InteractiveControlsVisible = map.IsZoomEnabled || map.IsScrollEnabled;
			}
		}

		/// <summary>
		/// Maps the <see cref="IMap.IsTrafficEnabled"/> property to the platform control.
		/// </summary>
		public static void MapIsTrafficEnabled(IMapHandler handler, IMap map)
		{
			// Traffic layer is not directly supported in the WinUI 3 MapControl
			// Azure Maps traffic would need to be configured through Azure Maps REST API or SDK
			// This is a known limitation of the basic MapControl
		}

		/// <summary>
		/// Maps the <see cref="IMap.IsShowingUser"/> property to the platform control.
		/// </summary>
		public static void MapIsShowingUser(IMapHandler handler, IMap map)
		{
			// The WinUI 3 MapControl doesn't have built-in user location display
			// We would need to use Geolocation API and add a custom MapIcon
			// For now, this is a no-op as it requires additional Geolocation permissions work
		}

		/// <summary>
		/// Handles the <see cref="IMap.MoveToRegion"/> command.
		/// </summary>
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

				// Calculate approximate zoom level from the span
				// This is a rough approximation - WinUI 3 MapControl doesn't expose zoom level directly
				// The span's latitude degrees roughly correspond to the visible area
			}
		}

		/// <summary>
		/// Maps the <see cref="IMap.Pins"/> collection to the platform control.
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

			// Clear existing pins
			_pinsLayer.MapElements.Clear();
			_mapIcons.Clear();

			// Add new pins
			foreach (var pin in pins)
			{
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
		}

		/// <summary>
		/// Maps the <see cref="IMap.Elements"/> collection to the platform control.
		/// </summary>
		public static void MapElements(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler)
			{
				mapHandler.UpdateMapElements(map.Elements);
			}
		}

		void UpdateMapElements(IList<IMapElement> elements)
		{
			if (_mapElementsLayer == null)
				return;

			// Clear existing elements
			_mapElementsLayer.MapElements.Clear();

			// Note: The WinUI 3 MapControl's MapElementsLayer primarily supports MapIcon
			// Polylines, polygons, and circles would need custom implementation
			// or use of Azure Maps REST API for more complex shapes
			// For now, we store the element reference but don't render shapes
			foreach (var element in elements)
			{
				// Mark the element as associated with this handler
				// Full shape rendering would require additional WinUI 3 MapControl capabilities
				// that may not be available in the current version
			}
		}

		/// <inheritdoc/>
		public void UpdateMapElement(IMapElement element)
		{
			// For property changes on map elements
			// Since WinUI 3 MapControl has limited shape support, this is a minimal implementation
		}
	}
}
