using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;
using Xamarin.Forms.Platform.WinPhone;

namespace Xamarin.Forms.Maps.WP8
{
	public class
		MapRenderer : ViewRenderer<Map, Microsoft.Phone.Maps.Controls.Map>
	{
		bool _firstZoomLevelChangeFired;
		MapLayer _pushPinLayer;
		MapLayer _userLocationLayer;

		protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
		{
			base.OnElementChanged(e);

			SetNativeControl(new Microsoft.Phone.Maps.Controls.Map());
			UpdateMapType();

			Control.Loaded += ControlOnLoaded;

			_pushPinLayer = new MapLayer();
			Control.Layers.Add(_pushPinLayer);

			_userLocationLayer = new MapLayer();
			Control.Layers.Add(_userLocationLayer);

			Control.ViewChanged += (s, a) => UpdateVisibleRegion();
			Control.ZoomLevelChanged += (sender, args) => UpdateVisibleRegion();
			Control.CenterChanged += (s, a) => UpdateVisibleRegion();
			//Control.ViewChangeOnFrame += (s, a) => UpdateVisibleRegion ();

			MessagingCenter.Subscribe<Map, MapSpan>(this, "MapMoveToRegion", (s, a) => MoveToRegion(a), Element);

			((ObservableCollection<Pin>)Element.Pins).CollectionChanged += OnCollectionChanged;

			if (Element.Pins.Any())
				LoadPins();

			UpdateShowUserLocation();
		}

		async void UpdateShowUserLocation()
		{
			if (Element.IsShowingUser)
			{
				var myGeolocator = new Geolocator();

				if (myGeolocator.LocationStatus != PositionStatus.NotAvailable &&
				    myGeolocator.LocationStatus != PositionStatus.Disabled)
				{
					var userPosition = await myGeolocator.GetGeopositionAsync();
					if (userPosition?.Coordinate != null)
						LoadUserPosition(userPosition.Coordinate, true);
				}
			}
			else if (_userLocationLayer.Count > 0)
				_userLocationLayer.Clear();
		}

		void ControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			MapsSettings.ApplicationContext.ApplicationId = FormsMaps.ApplicationId;
			MapsSettings.ApplicationContext.AuthenticationToken = FormsMaps.AuthenticationToken;
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (Pin pin in e.NewItems)
						LoadPin(pin);
					break;
				case NotifyCollectionChangedAction.Move:
					// no matter
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (Pin pin in e.OldItems)
						RemovePin(pin);
					break;
				case NotifyCollectionChangedAction.Replace:
					foreach (Pin pin in e.OldItems)
						RemovePin(pin);
					foreach (Pin pin in e.NewItems)
						LoadPin(pin);
					break;
				case NotifyCollectionChangedAction.Reset:
					_pushPinLayer.Clear();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void UpdateVisibleRegion()
		{
			if (!_firstZoomLevelChangeFired)
			{
				MoveToRegion(Element.LastMoveToRegion, MapAnimationKind.None);
				_firstZoomLevelChangeFired = true;
				return;
			}

			var center = new Position(Control.Center.Latitude, Control.Center.Longitude);
			var topLeft = Control.ConvertViewportPointToGeoCoordinate(new System.Windows.Point(0, 0));
			var bottomRight =
				Control.ConvertViewportPointToGeoCoordinate(new System.Windows.Point(Control.ActualWidth, Control.ActualHeight));
			if (topLeft == null || bottomRight == null)
				return;

			var boundingRegion = LocationRectangle.CreateBoundingRectangle(topLeft, bottomRight);
			var result = new MapSpan(center, boundingRegion.HeightInDegrees, boundingRegion.WidthInDegrees);
			Element.SetVisibleRegion(result);
		}

		void LoadPins()
		{
			foreach (var pin in Element.Pins)
				LoadPin(pin);
		}

		void LoadPin(Pin pin)
		{
			var location = new GeoCoordinate(pin.Position.Latitude, pin.Position.Longitude);
			var pushPin = new Pushpin
			{
				Content = pin.Label,
				PositionOrigin = new System.Windows.Point(0, 1),
				Tag = pin
			};

			pushPin.Tap += PinTap;

			var pushPinOverlay = new MapOverlay
			{
				Content = pushPin,
				GeoCoordinate = location,
				PositionOrigin = new System.Windows.Point(0, 1)
			};
			_pushPinLayer.Add(pushPinOverlay);
		}

		void PinTap(object sender, GestureEventArgs e)
		{
			var pushPin = (Pushpin)sender;
			var pin = (Pin)pushPin.Tag;
			pin.SendTap();
		}

		void RemovePin(Pin pin)
		{
			var child = _pushPinLayer.FirstOrDefault(p => ((Pushpin)p.Content).Tag == (object)pin);
			if (child != null)
				_pushPinLayer.Remove(child);
		}

		void MoveToRegion(MapSpan span, MapAnimationKind animation = MapAnimationKind.Parabolic)
		{
			// FIXME
			var center = new GeoCoordinate(span.Center.Latitude, span.Center.Longitude);
			var location = new LocationRectangle(center, span.LongitudeDegrees, span.LatitudeDegrees);
			Control.SetView(location, animation);
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return new SizeRequest(new Size(100, 100));
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Map.MapTypeProperty.PropertyName)
				UpdateMapType();
			if (e.PropertyName == Map.IsShowingUserProperty.PropertyName)
				UpdateShowUserLocation();
		}

		void UpdateMapType()
		{
			switch (Element.MapType)
			{
				case MapType.Street:
					Control.CartographicMode = MapCartographicMode.Road;
					break;
				case MapType.Satellite:
					Control.CartographicMode = MapCartographicMode.Aerial;
					break;
				case MapType.Hybrid:
					Control.CartographicMode = MapCartographicMode.Hybrid;
					break;
			}
		}

		void LoadUserPosition(Geocoordinate userPosition, bool center = false)
		{
			_userLocationLayer.Clear();

			var userCoordinate = new GeoCoordinate
				(
				userPosition.Latitude,
				userPosition.Longitude,
				userPosition.Altitude ?? double.NaN,
				userPosition.Accuracy,
				userPosition.AltitudeAccuracy ?? double.NaN,
				userPosition.Speed ?? double.NaN,
				userPosition.Heading ?? double.NaN
				);

			//make some preety?
			var userPositionCircle = new Ellipse
			{
				Fill = new SolidColorBrush(Colors.Blue),
				Height = 20,
				Width = 20,
				Opacity = 50
			};

			var userPostionOverlay = new MapOverlay
			{
				Content = userPositionCircle,
				PositionOrigin = new System.Windows.Point(0.5, 0.5),
				GeoCoordinate = userCoordinate
			};

			_userLocationLayer.Add(userPostionOverlay);

			if (center)
			{
				Control.Center = userCoordinate;
				Control.ZoomLevel = 13;
			}
		}
	}
}