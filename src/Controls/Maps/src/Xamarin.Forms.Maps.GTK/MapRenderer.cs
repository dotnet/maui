using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Timers;
using System.Windows.Forms.Markers;
using GMap.NET;
using GMap.NET.GTK;
using GMap.NET.MapProviders;
using Newtonsoft.Json;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK;

namespace Xamarin.Forms.Maps.GTK
{
	public class MapRenderer : ViewRenderer<Map, GMapControl>
	{
		private const string OverlayId = "overlayId";
		private const int MinZoom = 0;
		private const int MaxZoom = 24;

		private GMapImageMarker _userPosition;
		private bool _disposed;
		private Timer _timer;

		protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
		{
			if (e.OldElement != null)
			{
				var mapModel = e.OldElement;
				MessagingCenter.Unsubscribe<Map, MapSpan>(this, "MapMoveToRegion");
				((System.Collections.ObjectModel.ObservableCollection<Pin>)mapModel.Pins).CollectionChanged -= OnCollectionChanged;
			}

			if (e.NewElement != null)
			{
				var mapModel = e.NewElement;

				if (Control == null)
				{
					var gMapControl = new GMapControl();
					GMapProviders.GoogleMap.ApiKey = FormsMaps.AuthenticationToken;
					gMapControl.MinZoom = MinZoom;
					gMapControl.MaxZoom = MaxZoom;
					gMapControl.SelectedAreaFillColor = System.Drawing.Color.Transparent;

					gMapControl.Overlays.Add(new GMapOverlay(OverlayId));

					SetNativeControl(gMapControl);

					Control.SizeAllocated += OnSizeAllocated;
					Control.OnPositionChanged += OnPositionChanged;
					Control.OnMapZoomChanged += OnMapZoomChanged;
					Control.ButtonPressEvent += OnButtonPressEvent;
					Control.OnMarkerClick += OnMarkerClick;
				}

				MessagingCenter.Subscribe<Maps.Map, MapSpan>(this, "MapMoveToRegion", (s, a) =>
				MoveToRegion(a), mapModel);

				UpdateMapType();
				UpdateHasScrollEnabled();
				UpdateHasZoomEnabled();

				((System.Collections.ObjectModel.ObservableCollection<Pin>)mapModel.Pins).CollectionChanged += OnCollectionChanged;

				if (mapModel.Pins.Any())
					LoadPins();

				if (Control == null)
					return;

				MoveToRegion(mapModel.LastMoveToRegion);
				UpdateIsShowingUser();
			}

			base.OnElementChanged(e);
		}

		private void OnSizeAllocated(object o, Gtk.SizeAllocatedArgs args)
		{
			UpdateVisibleRegion();
		}

		private void OnPositionChanged(PointLatLng point)
		{
			UpdateVisibleRegion();
		}

		private void OnMapZoomChanged()
		{
			UpdateVisibleRegion();
		}

		private void OnButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
		{
			Control.SelectionPen = new Pen(Brushes.Black, 2);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Maps.Map.MapTypeProperty.PropertyName)
				UpdateMapType();
			else if (e.PropertyName == Maps.Map.IsShowingUserProperty.PropertyName)
				UpdateIsShowingUser();
			else if (e.PropertyName == Maps.Map.HasScrollEnabledProperty.PropertyName)
				UpdateHasScrollEnabled();
			else if (e.PropertyName == Maps.Map.HasZoomEnabledProperty.PropertyName)
				UpdateHasZoomEnabled();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;

				MessagingCenter.Unsubscribe<Maps.Map, MapSpan>(this, "MapMoveToRegion");

				if (Control != null)
				{
					Control.SizeAllocated -= OnSizeAllocated;
					Control.OnPositionChanged -= OnPositionChanged;
					Control.OnMapZoomChanged -= OnMapZoomChanged;
					Control.ButtonPressEvent -= OnButtonPressEvent;
					Control.OnMarkerClick -= OnMarkerClick;
				}

				if (Element != null)
					((System.Collections.ObjectModel.ObservableCollection<Pin>)Element.Pins).CollectionChanged -= OnCollectionChanged;
			}

			base.Dispose(disposing);
		}

		private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					foreach (Pin pin in e.NewItems)
						LoadPin(pin);
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
					// Do nothing
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					foreach (Pin pin in e.OldItems)
						RemovePin(pin);
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					foreach (Pin pin in e.OldItems)
						RemovePin(pin);
					foreach (Pin pin in e.NewItems)
						LoadPin(pin);
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
					ClearPins();
					break;
			}
		}

		private void LoadPins()
		{
			foreach (var pin in Element.Pins)
				LoadPin(pin);
		}

		private void LoadPin(Pin pin)
		{
			var overlay = Control.Overlays.FirstOrDefault();

			if (overlay != null)
			{
				var gMapImageMarker = new GMapImageMarker(
					new PointLatLng(
						pin.Position.Latitude,
						pin.Position.Longitude),
					GMapImageMarkerType.RedDot);

				overlay.Markers.Add(gMapImageMarker);
			}
		}

		private void RemovePin(Pin pinToRemove)
		{
			var overlay = Control.Overlays.FirstOrDefault();

			if (overlay != null)
			{
				var positionToRemove = new PointLatLng(
						pinToRemove.Position.Latitude,
						pinToRemove.Position.Longitude);

				var pins = overlay.Markers.Where(p => p.Position == positionToRemove);

				foreach (var pin in pins)
				{
					overlay.Markers.Remove(pin);
				}
			}
		}

		private void ClearPins()
		{
			var overlay = Control.Overlays.FirstOrDefault();

			if (overlay != null)
			{
				overlay.Markers.Clear();
			}

			UpdateIsShowingUser();
		}

		private void UpdateMapType()
		{
			switch (Element.MapType)
			{
				case MapType.Street:
					Control.MapProvider = GMapProviders.GoogleMap;
					break;
				case MapType.Satellite:
					Control.MapProvider = GMapProviders.GoogleSatelliteMap;
					break;
				case MapType.Hybrid:
					Control.MapProvider = GMapProviders.GoogleHybridMap;
					break;
			}
		}

		private void UpdateIsShowingUser(bool moveToLocation = true)
		{
			if (Control == null || Element == null)
				return;

			var overlay = Control.Overlays.FirstOrDefault();

			if (Element.IsShowingUser)
			{
				var userPosition = GetUserPosition();

				if (userPosition != null)
				{
					LoadUserPosition(userPosition.Value, moveToLocation);
				}

				if (Control == null || Element == null)
					return;

				if (_timer == null)
				{
					_timer = new Timer();
					_timer.Elapsed += (s, o) => UpdateIsShowingUser();
					_timer.Interval = 1000;
				}

				if (!_timer.Enabled)
					_timer.Start();
			}
			else if (_userPosition != null && overlay.Markers.Contains(_userPosition))
			{
				_timer.Stop();
				overlay.Markers.Remove(_userPosition);
			}
		}

		private void LoadUserPosition(PointLatLng userCoordinate, bool center)
		{
			if (Control == null || Element == null)
				return;

			if (_userPosition == null)
			{
				_userPosition = new GMapImageMarker(userCoordinate, GMapImageMarkerType.Red);
			}

			var overlay = Control.Overlays.FirstOrDefault();

			if (overlay != null)
			{
				overlay.Markers.Add(_userPosition);
			}

			if (center)
			{
				Control.Position = userCoordinate;
			}
		}

		private PointLatLng? GetUserPosition()
		{
			try
			{
				var ipAddress = GetPublicIpAddress();
				var webClient = new WebClient();
				var uri = new Uri(string.Format("http://freegeoip.net/json/{0}", ipAddress.ToString()));
				var result = webClient.DownloadString(uri);
				var location = JsonConvert.DeserializeObject<Location>(result);

				return new PointLatLng(
					location.Latitude,
					location.Longitude);
			}
			catch
			{
				return null;
			}
		}

		private string GetPublicIpAddress()
		{
			string uri = "http://checkip.dyndns.org/";
			string ip = string.Empty;

			using (var client = new HttpClient())
			{
				var result = client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;

				ip = result.Split(':')[1].Split('<')[0];
				ip = ip.Trim();
			}

			return ip;
		}

		private void UpdateHasScrollEnabled()
		{
			var hasScrollEnabled = Element.HasScrollEnabled;

			Control.CanDragMap = hasScrollEnabled;
		}

		private void UpdateHasZoomEnabled()
		{
			var hasZoomEnabled = Element.HasZoomEnabled;

			Control.MouseWheelZoomEnabled = hasZoomEnabled;
		}

		private void MoveToRegion(MapSpan span)
		{
			try
			{
				if (span == null)
				{
					return;
				}

				var p1 = new PointLatLng
				{
					Lat = span.Center.Latitude + span.LatitudeDegrees / 2,
					Lng = span.Center.Longitude - span.LongitudeDegrees / 2
				};

				var p2 = new PointLatLng
				{
					Lat = span.Center.Latitude - span.LatitudeDegrees / 2,
					Lng = span.Center.Longitude + span.LongitudeDegrees / 2
				};

				double x1 = Math.Min(p1.Lng, p2.Lng);
				double y1 = Math.Max(p1.Lat, p2.Lat);
				double x2 = Math.Max(p1.Lng, p2.Lng);
				double y2 = Math.Min(p1.Lat, p2.Lat);

				var region = new RectLatLng(y1, x1, x2 - x1, y1 - y2);

				Control.SelectionPen = new Pen(Brushes.Transparent, 2);
				Control.SelectedArea = region;
				Control.SetZoomToFitRect(region);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("MoveToRegion exception: " + ex);
				Log.Warning("Xamarin.Forms MapRenderer", $"MoveToRegion exception: {ex}");
			}
		}

		private void UpdateVisibleRegion()
		{
			if (Control == null || Element == null)
				return;

			try
			{
				var region = Control.SelectedArea;
				var topLeft = region.LocationTopLeft;
				var center = region.LocationMiddle;
				var rightBottom = region.LocationRightBottom;

				var latitudeDelta = Math.Abs(topLeft.Lat - rightBottom.Lat);
				var longitudeDelta = Math.Abs(topLeft.Lng - rightBottom.Lng);

				Element.SetVisibleRegion(new MapSpan(
					new Position(center.Lat, center.Lng),
					latitudeDelta,
					longitudeDelta));
			}
			catch (Exception ex)
			{
				Debug.WriteLine("UpdateVisibleRegion exception: " + ex);
				Log.Warning("Xamarin.Forms MapRenderer", $"UpdateVisibleRegion exception: {ex}");

				return;
			}
		}

		private void OnMarkerClick(GMap.NET.GTK.Markers.GMapMarker item)
		{
			var marker = item;

			foreach (var pin in Element.Pins)
			{
				if (pin.Position.Latitude == marker.Position.Lat &&
					pin.Position.Longitude == marker.Position.Lng)
				{
#pragma warning disable CS0618
					pin.SendTap();
#pragma warning restore CS0618
					break;
				}
			}
		}
	}
}