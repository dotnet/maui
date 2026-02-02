using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Common.Apis;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Views;
using Java.Lang;
using Java.Util.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Maps.Platform;
using Microsoft.Maui.Platform;
using ABitmap = Android.Graphics.Bitmap;
using ACanvas = Android.Graphics.Canvas;
using APaint = Android.Graphics.Paint;
using ARect = Android.Graphics.Rect;
using ACircle = Android.Gms.Maps.Model.Circle;
using APolygon = Android.Gms.Maps.Model.Polygon;
using APolyline = Android.Gms.Maps.Model.Polyline;
using Math = System.Math;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapHandler : ViewHandler<IMap, MapView>
	{
		bool _init = true;

		MapCallbackHandler? _mapReady;
		MapSpan? _lastMoveToRegion;
		List<Marker>? _markers;
		IList? _pins;
		IList? _elements;
		List<APolyline>? _polylines;
		List<APolygon>? _polygons;
		List<ACircle>? _circles;
		bool _isClusteringEnabled;
		List<MapCluster>? _clusters;
		Dictionary<string, MapCluster>? _clusterMarkers;

		public GoogleMap? Map { get; private set; }

		static Bundle? s_bundle;

		public static Bundle? Bundle
		{
			set { s_bundle = value; }
		}

		protected override void ConnectHandler(MapView platformView)
		{
			base.ConnectHandler(platformView);
			_mapReady = new MapCallbackHandler(this);
			platformView.GetMapAsync(_mapReady);
			platformView.LayoutChange += MapViewLayoutChange;
		}

		protected override void DisconnectHandler(MapView platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.LayoutChange -= MapViewLayoutChange;

			if (Map != null)
			{
				Map.SetOnCameraMoveListener(null);
				Map.MarkerClick -= OnMarkerClick;
				Map.InfoWindowClick -= OnInfoWindowClick;
				Map.MapClick -= OnMapClick;
			}

			_clusters?.Clear();
			_clusterMarkers?.Clear();
			_mapReady = null;
		}

		protected override MapView CreatePlatformView()
		{
			MapView mapView = new MapView(Context);
			mapView.OnCreate(s_bundle);
			mapView.OnResume();
			return mapView;
		}

		public static void MapMapType(IMapHandler handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			googleMap?.UpdateMapType(map);
		}

		public static void MapIsShowingUser(IMapHandler handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			googleMap?.UpdateIsShowingUser(map, handler?.MauiContext);
		}

		public static void MapIsScrollEnabled(IMapHandler handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			googleMap?.UpdateIsScrollEnabled(map);
		}

		public static void MapIsTrafficEnabled(IMapHandler handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			googleMap?.UpdateIsTrafficEnabled(map);
		}

		public static void MapIsZoomEnabled(IMapHandler handler, IMap map)
		{
			GoogleMap? googleMap = handler?.Map;
			googleMap?.UpdateIsZoomEnabled(map);
		}

		public static void MapIsClusteringEnabled(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler)
			{
				mapHandler._isClusteringEnabled = map.IsClusteringEnabled;
				// Re-add pins to apply clustering changes
				MapPins(handler, map);
			}
		}

		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg)
		{
			MapSpan? newRegion = arg as MapSpan;
			if (newRegion != null)
				(handler as MapHandler)?.MoveToRegion(newRegion, true);
		}

		public void UpdateMapElement(IMapElement element)
		{
			switch (element)
			{
				case IGeoPathMapElement polyline:
					{
						if (element is IFilledMapElement polygon)
						{
							PolygonOnPropertyChanged(polyline);
						}
						else
						{
							PolylineOnPropertyChanged(polyline);
						}
						break;
					}
				case ICircleMapElement circle:
					{
						CircleOnPropertyChanged(circle);
						break;
					}
			}
		}

		void PolygonOnPropertyChanged(IGeoPathMapElement mauiPolygon)
		{
			var nativePolygon = GetNativePolygon(mauiPolygon);

			if (nativePolygon == null)
				return;

			if (mauiPolygon.Stroke is SolidPaint solidPaint)
				nativePolygon.StrokeColor = solidPaint.Color.AsColor();

			if ((mauiPolygon as IFilledMapElement)?.Fill is SolidPaint solidFillPaint)
				nativePolygon.FillColor = solidFillPaint.Color.AsColor();

			nativePolygon.StrokeWidth = (float)mauiPolygon.StrokeThickness;
			nativePolygon.Points = mauiPolygon.Select(position => new LatLng(position.Latitude, position.Longitude)).ToList();
		}

		void PolylineOnPropertyChanged(IGeoPathMapElement mauiPolyline)
		{
			var nativePolyline = GetNativePolyline(mauiPolyline);

			if (nativePolyline == null)
				return;

			if (mauiPolyline.Stroke is SolidPaint solidPaint)
				nativePolyline.Color = solidPaint.Color.AsColor();

			nativePolyline.Width = (float)mauiPolyline.StrokeThickness;
			nativePolyline.Points = mauiPolyline.Select(position => new LatLng(position.Latitude, position.Longitude)).ToList();
		}


		void CircleOnPropertyChanged(ICircleMapElement mauiCircle)
		{
			var nativeCircle = GetNativeCircle(mauiCircle);

			if (nativeCircle == null)
				return;


			if (mauiCircle.Stroke is SolidPaint solidPaint)
				nativeCircle.FillColor = solidPaint.Color.AsColor();

			if (mauiCircle.Fill is SolidPaint solidFillPaint)
				nativeCircle.FillColor = solidFillPaint.Color.AsColor();

			nativeCircle.Center = new LatLng(mauiCircle.Center.Latitude, mauiCircle.Center.Longitude);
			nativeCircle.Radius = mauiCircle.Radius.Meters;
			nativeCircle.StrokeWidth = (float)mauiCircle.StrokeThickness;

		}

		protected APolyline? GetNativePolyline(IGeoPathMapElement polyline)
		{
			APolyline? targetPolyline = null;

			if (_polylines != null && polyline.MapElementId is string)
			{
				for (int i = 0; i < _polylines.Count; i++)
				{
					var native = _polylines[i];
					if (native.Id == (string)polyline.MapElementId)
					{
						targetPolyline = native;
						break;
					}
				}
			}

			return targetPolyline;
		}

		protected ACircle? GetNativeCircle(ICircleMapElement circle)
		{
			ACircle? targetCircle = null;

			if (_circles != null && circle.MapElementId is string)
			{
				for (int i = 0; i < _circles.Count; i++)
				{
					var native = _circles[i];
					if (native.Id == (string)circle.MapElementId)
					{
						targetCircle = native;
						break;
					}
				}
			}

			return targetCircle;
		}

		protected APolygon? GetNativePolygon(IGeoPathMapElement polygon)
		{
			APolygon? targetPolygon = null;

			if (_polygons != null && polygon.MapElementId is string)
			{
				for (int i = 0; i < _polygons.Count; i++)
				{
					var native = _polygons[i];
					if (native.Id == (string)polygon.MapElementId)
					{
						targetPolygon = native;
						break;
					}
				}
			}

			return targetPolygon;
		}

		public static void MapPins(IMapHandler handler, IMap map)
		{
			if (handler is MapHandler mapHandler)
			{
				if (mapHandler._markers != null)
				{
					for (int i = 0; i < mapHandler._markers.Count; i++)
						mapHandler._markers[i].Remove();

					mapHandler._markers = null;
				}

				mapHandler.AddPins((IList)map.Pins);
			}
		}

		public static void MapElements(IMapHandler handler, IMap map)
		{
			(handler as MapHandler)?.ClearMapElements();
			(handler as MapHandler)?.AddMapElements((IList)map.Elements);
		}

		/// <summary>
		/// Clusters pins based on their proximity using a simple grid-based algorithm.
		/// </summary>
		List<MapCluster> CreateClusters(IList<IMapPin> pins, float zoom)
		{
			var clusters = new List<MapCluster>();
			if (pins == null || pins.Count == 0)
				return clusters;

			// Cluster radius in degrees, adjusted based on zoom level
			// At zoom 0 (world view), cluster radius is large
			// At zoom 21 (street level), cluster radius is tiny
			// The divisor controls how aggressively pins cluster - smaller = less clustering
			// 50 pixels on screen ≈ cluster if pins would overlap at this zoom
			double clusterRadius = 50.0 / Math.Pow(2, zoom);

			// Group pins by their clustering identifier
			var pinsByIdentifier = new Dictionary<string, List<IMapPin>>();
			foreach (var pin in pins)
			{
				var identifier = pin.ClusteringIdentifier ?? "default";
				if (!pinsByIdentifier.ContainsKey(identifier))
					pinsByIdentifier[identifier] = new List<IMapPin>();
				pinsByIdentifier[identifier].Add(pin);
			}

			// Create clusters for each group
			foreach (var group in pinsByIdentifier.Values)
			{
				var remaining = new List<IMapPin>(group);
				
				while (remaining.Count > 0)
				{
					var seed = remaining[0];
					remaining.RemoveAt(0);
					
					var cluster = new MapCluster(seed.Location.Latitude, seed.Location.Longitude);
					cluster.Pins.Add(seed);
					
					// Find all pins within radius
					for (int i = remaining.Count - 1; i >= 0; i--)
					{
						var pin = remaining[i];
						var distance = Math.Sqrt(
							Math.Pow(pin.Location.Latitude - cluster.CenterLatitude, 2) +
							Math.Pow(pin.Location.Longitude - cluster.CenterLongitude, 2));
						
						if (distance <= clusterRadius)
						{
							cluster.Pins.Add(pin);
							remaining.RemoveAt(i);
							
							// Update cluster center to be the centroid
							double sumLat = 0, sumLng = 0;
							foreach (var p in cluster.Pins)
							{
								sumLat += p.Location.Latitude;
								sumLng += p.Location.Longitude;
							}
							cluster.CenterLatitude = sumLat / cluster.Pins.Count;
							cluster.CenterLongitude = sumLng / cluster.Pins.Count;
						}
					}
					
					clusters.Add(cluster);
				}
			}

			return clusters;
		}

		internal void OnMapReady(GoogleMap map)
		{
			if (map == null)
			{
				return;
			}

			Map = map;

			map.SetOnCameraMoveListener(_mapReady);
			map.SetOnCameraIdleListener(_mapReady);

			map.MarkerClick += OnMarkerClick;
			map.InfoWindowClick += OnInfoWindowClick;
			map.MapClick += OnMapClick;

			if (VirtualView != null)
			{
				map.UpdateMapType(VirtualView);
				map.UpdateIsShowingUser(VirtualView, MauiContext);
				map.UpdateIsScrollEnabled(VirtualView);
				map.UpdateIsTrafficEnabled(VirtualView);
				map.UpdateIsZoomEnabled(VirtualView);
			}

			InitialUpdate();
		}

		/// <summary>
		/// Recalculates clusters based on current zoom level.
		/// Called when camera becomes idle after zooming.
		/// </summary>
		internal void ReclusterPins()
		{
			if (!_isClusteringEnabled || Map == null || VirtualView == null)
				return;

			// Clear existing markers
			if (_markers != null)
			{
				for (int i = 0; i < _markers.Count; i++)
					_markers[i].Remove();
				_markers = null;
			}
			_clusterMarkers?.Clear();

			// Re-add pins to trigger reclustering at new zoom level
			AddPins((IList)VirtualView.Pins);
		}

		internal void UpdateVisibleRegion(LatLng pos)
		{
			if (Map == null)
				return;

			Projection projection = Map.Projection;
			int width = PlatformView.Width;
			int height = PlatformView.Height;
			LatLng ul = projection.FromScreenLocation(new global::Android.Graphics.Point(0, 0));
			LatLng ur = projection.FromScreenLocation(new global::Android.Graphics.Point(width, 0));
			LatLng ll = projection.FromScreenLocation(new global::Android.Graphics.Point(0, height));
			LatLng lr = projection.FromScreenLocation(new global::Android.Graphics.Point(width, height));
			double dlat = Math.Max(Math.Abs(ul.Latitude - lr.Latitude), Math.Abs(ur.Latitude - ll.Latitude));
			double dlong = Math.Max(Math.Abs(ul.Longitude - lr.Longitude), Math.Abs(ur.Longitude - ll.Longitude));
			VirtualView.VisibleRegion = new MapSpan(new Devices.Sensors.Location(pos.Latitude, pos.Longitude), dlat, dlong);
		}

		void MapViewLayoutChange(object? sender, View.LayoutChangeEventArgs e)
		{
			InitialUpdate();
		}

		void InitialUpdate()
		{
			if (Map == null)
				return;

			if (_init && _lastMoveToRegion != null)
			{
				MoveToRegion(_lastMoveToRegion, false);
				if (_pins != null)
					AddPins(_pins);
				if (_elements != null)
					AddMapElements(_elements);
				_init = false;
			}

			UpdateVisibleRegion(Map.CameraPosition.Target);
		}

		void MoveToRegion(MapSpan span, bool animate)
		{
			_lastMoveToRegion = span;
			if (Map == null)
				return;

			var ne = new LatLng(span.Center.Latitude + span.LatitudeDegrees / 2,
				span.Center.Longitude + span.LongitudeDegrees / 2);
			var sw = new LatLng(span.Center.Latitude - span.LatitudeDegrees / 2,
				span.Center.Longitude - span.LongitudeDegrees / 2);
			CameraUpdate update = CameraUpdateFactory.NewLatLngBounds(new LatLngBounds(sw, ne), 0);

			try
			{
				if (animate)
				{
					Map.AnimateCamera(update);
				}
				else
				{
					Map.MoveCamera(update);
				}
			}
			catch (IllegalStateException exc)
			{
				MauiContext?.Services.GetService<ILogger<MapHandler>>()?.LogWarning(exc, $"MoveToRegion exception");
			}
		}

		void OnMarkerClick(object? sender, GoogleMap.MarkerClickEventArgs e)
		{
			// Check if this is a cluster marker
			if (_clusterMarkers != null && _clusterMarkers.TryGetValue(e.Marker.Id, out var cluster))
			{
				var location = new Devices.Sensors.Location(cluster.CenterLatitude, cluster.CenterLongitude);
				bool handled = VirtualView.ClusterClicked(cluster.Pins, location);
				
				if (!handled && Map != null)
				{
					// Default behavior: zoom in to show cluster pins
					var builder = new LatLngBounds.Builder();
					foreach (var clusterPin in cluster.Pins)
					{
						builder.Include(new LatLng(clusterPin.Location.Latitude, clusterPin.Location.Longitude));
					}
					
					try
					{
						var bounds = builder.Build();
						var update = CameraUpdateFactory.NewLatLngBounds(bounds, 100);
						Map.AnimateCamera(update);
					}
					catch (IllegalStateException)
					{
						// Bounds couldn't be built
					}
				}
				
				e.Handled = true;
				return;
			}

			var pin = GetPinForMarker(e.Marker);

			if (pin == null)
			{
				return;
			}

			// Setting e.Handled = true will prevent the info window from being presented
			// SendMarkerClick() returns the value of PinClickedEventArgs.HideInfoWindow
			bool markerHandled = pin.SendMarkerClick();
			e.Handled = markerHandled;
		}

		void OnInfoWindowClick(object? sender, GoogleMap.InfoWindowClickEventArgs e)
		{
			var marker = e.Marker;
			
			// Cluster markers don't have info windows
			if (_clusterMarkers != null && _clusterMarkers.ContainsKey(marker.Id))
				return;
			
			var pin = GetPinForMarker(marker);

			if (pin == null)
			{
				return;
			}

			pin.SendMarkerClick();

			// SendInfoWindowClick() returns the value of PinClickedEventArgs.HideInfoWindow
			bool hideInfoWindow = pin.SendInfoWindowClick();
			if (hideInfoWindow)
			{
				marker.HideInfoWindow();
			}
		}

		void OnMapClick(object? sender, GoogleMap.MapClickEventArgs e) =>
			VirtualView.Clicked(new Devices.Sensors.Location(e.Point.Latitude, e.Point.Longitude));

		void AddPins(IList pins)
		{
			//Mapper could be called before we have a Map ready 
			_pins = pins;
			if (Map == null || MauiContext == null)
				return;

			if (_markers == null)
				_markers = new List<Marker>();

			if (_clusterMarkers == null)
				_clusterMarkers = new Dictionary<string, MapCluster>();
			else
				_clusterMarkers.Clear();

			// Convert to IMapPin list
			var mapPins = new List<IMapPin>();
			foreach (var p in pins)
			{
				mapPins.Add((IMapPin)p);
			}

			// When clustering is enabled, create clusters
			if (_isClusteringEnabled && mapPins.Count > 0)
			{
				float zoom = Map.CameraPosition?.Zoom ?? 10f;
				_clusters = CreateClusters(mapPins, zoom);

				foreach (var cluster in _clusters)
				{
					if (cluster.Pins.Count == 1)
					{
						// Single pin - add as regular marker
						var pin = cluster.Pins[0];
						var pinHandler = pin.ToHandler(MauiContext);
						if (pinHandler is IMapPinHandler iMapPinHandler)
						{
							var marker = Map.AddMarker(iMapPinHandler.PlatformView);
							if (marker != null)
							{
								pin.MarkerId = marker.Id;
								_markers.Add(marker);
							}
						}
					}
					else
					{
						// Multiple pins - create cluster marker
						var options = new MarkerOptions();
						options.SetPosition(new LatLng(cluster.CenterLatitude, cluster.CenterLongitude));
						options.SetTitle($"{cluster.Pins.Count} items");
						options.Anchor(0.5f, 0.5f);
						
						// Create a circular cluster icon with count
						var icon = CreateClusterIcon(cluster.Pins.Count);
						if (icon != null)
							options.SetIcon(icon);
						
						var marker = Map.AddMarker(options);
						if (marker != null)
						{
							_markers.Add(marker);
							_clusterMarkers[marker.Id] = cluster;
						}
					}
				}
				
				_pins = null;
				return;
			}

			// Normal non-clustered pins
			foreach (var p in pins)
			{
				IMapPin pin = (IMapPin)p;
				Marker? marker;

				var pinHandler = pin.ToHandler(MauiContext);
				if (pinHandler is IMapPinHandler iMapPinHandler)
				{
					marker = Map.AddMarker(iMapPinHandler.PlatformView);
					if (marker == null)
					{
						throw new System.Exception("Map.AddMarker returned null");
					}
					// associate pin with marker for later lookup in event handlers
					pin.MarkerId = marker.Id;
					_markers.Add(marker!);
				}

			}
			_pins = null;
		}

		BitmapDescriptor? CreateClusterIcon(int count)
		{
			try
			{
				int size = 100; // pixels
				var bitmap = ABitmap.CreateBitmap(size, size, ABitmap.Config.Argb8888!);
				if (bitmap == null)
					return null;
					
				var canvas = new ACanvas(bitmap);
				
				// Draw circle background
				var paint = new APaint();
				paint.AntiAlias = true;
				paint.SetARGB(255, 66, 133, 244); // Google blue
				canvas.DrawCircle(size / 2f, size / 2f, size / 2f - 2, paint);
				
				// Draw border
				paint.SetStyle(APaint.Style.Stroke);
				paint.StrokeWidth = 4;
				paint.SetARGB(255, 255, 255, 255); // White border
				canvas.DrawCircle(size / 2f, size / 2f, size / 2f - 4, paint);
				
				// Draw text
				paint.SetStyle(APaint.Style.Fill);
				paint.SetARGB(255, 255, 255, 255); // White text
				paint.TextSize = count > 99 ? 28 : 36;
				paint.TextAlign = APaint.Align.Center;
				
				var text = count > 99 ? "99+" : count.ToString();
				var textBounds = new ARect();
				paint.GetTextBounds(text, 0, text.Length, textBounds);
				
				float x = size / 2f;
				float y = size / 2f + textBounds.Height() / 2f;
				canvas.DrawText(text, x, y, paint);
				
				return BitmapDescriptorFactory.FromBitmap(bitmap);
			}
			catch
			{
				return null;
			}
		}

		protected IMapPin? GetPinForMarker(Marker marker)
		{
			IMapPin? targetPin = null;

			for (int i = 0; i < VirtualView.Pins.Count; i++)
			{
				var pin = VirtualView.Pins[i];
				if (pin?.MarkerId is string markerId)
				{
					if (markerId == marker.Id)
					{
						targetPin = pin;
						break;
					}
				}
			}

			return targetPin;
		}

		void ClearMapElements()
		{
			if (_polylines != null)
			{
				for (int i = 0; i < _polylines.Count; i++)
					_polylines[i].Remove();

				_polylines = null;
			}

			if (_polygons != null)
			{
				for (int i = 0; i < _polygons.Count; i++)
					_polygons[i].Remove();

				_polygons = null;
			}

			if (_circles != null)
			{
				for (int i = 0; i < _circles.Count; i++)
					_circles[i].Remove();

				_circles = null;
			}
		}

		void AddMapElements(IList mapElements)
		{
			_elements = mapElements;

			if (Map == null || MauiContext == null)
				return;

			foreach (var element in mapElements)
			{
				if (element is IGeoPathMapElement geoPath)
				{
					if (element is IFilledMapElement)
					{
						AddPolygon(geoPath);
					}
					else
					{
						AddPolyline(geoPath);
					}
				}
				if (element is ICircleMapElement circle)
				{
					AddCircle(circle);
				}
			}
			_elements = null;
		}

		void AddPolyline(IGeoPathMapElement polyline)
		{
			var map = Map;
			if (map == null)
				return;

			if (_polylines == null)
				_polylines = new List<APolyline>();

			var options = polyline.ToHandler(MauiContext!)?.PlatformView as PolylineOptions;
			if (options != null)
			{
				var nativePolyline = map.AddPolyline(options);

				polyline.MapElementId = nativePolyline.Id;

				_polylines.Add(nativePolyline);
			}
		}

		void AddPolygon(IGeoPathMapElement polygon)
		{
			var map = Map;
			if (map == null)
				return;

			if (_polygons == null)
				_polygons = new List<APolygon>();

			var options = polygon.ToHandler(MauiContext!)?.PlatformView as PolygonOptions;
			if (options is null)
			{
				throw new System.Exception("PolygonOptions is null");
			}
			var nativePolygon = map.AddPolygon(options);

			polygon.MapElementId = nativePolygon.Id;

			_polygons.Add(nativePolygon);
		}

		void AddCircle(ICircleMapElement circle)
		{
			var map = Map;
			if (map == null)
				return;

			if (_circles == null)
				_circles = new List<ACircle>();

			var options = circle.ToHandler(MauiContext!)?.PlatformView as CircleOptions;
			if (options is null)
			{
				throw new System.Exception("CircleOptions is null");
			}
			var nativeCircle = map.AddCircle(options);

			circle.MapElementId = nativeCircle.Id;

			_circles.Add(nativeCircle);
		}
	}

	class MapCallbackHandler : Java.Lang.Object, GoogleMap.IOnCameraMoveListener, GoogleMap.IOnCameraIdleListener, IOnMapReadyCallback
	{
		MapHandler? _handler;
		GoogleMap? _googleMap;
		float _lastClusterZoom = -1;

		public MapCallbackHandler(MapHandler mapHandler)
		{
			_handler = mapHandler;
		}

		public void OnMapReady(GoogleMap googleMap)
		{
			_googleMap = googleMap;
			_handler?.OnMapReady(googleMap);
		}

		void GoogleMap.IOnCameraMoveListener.OnCameraMove()
		{
			if (_googleMap == null)
				return;

			_handler?.UpdateVisibleRegion(_googleMap.CameraPosition.Target);
		}

		void GoogleMap.IOnCameraIdleListener.OnCameraIdle()
		{
			if (_googleMap == null || _handler == null)
				return;

			// Recalculate clusters when zoom level changes significantly
			var currentZoom = _googleMap.CameraPosition.Zoom;
			if (Math.Abs(currentZoom - _lastClusterZoom) > 0.5f)
			{
				_lastClusterZoom = currentZoom;
				_handler.ReclusterPins();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _handler != null)
			{
				_handler = null;
				_googleMap = null;
			}

			base.Dispose(disposing);
		}
	}

	/// <summary>
	/// Represents a cluster of map pins.
	/// </summary>
	class MapCluster
	{
		public MapCluster(double lat, double lng)
		{
			CenterLatitude = lat;
			CenterLongitude = lng;
			Pins = new List<IMapPin>();
		}

		public double CenterLatitude { get; set; }
		public double CenterLongitude { get; set; }
		public List<IMapPin> Pins { get; }
	}

}
