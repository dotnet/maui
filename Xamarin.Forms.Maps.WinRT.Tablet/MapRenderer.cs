using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Bing.Maps;
using Xamarin.Forms.Platform.WinRT;

namespace Xamarin.Forms.Maps.WinRT
{
	public class MapRenderer : ViewRenderer<Map, Bing.Maps.Map>
	{
		bool _disposed;
		bool _firstZoomLevelChangeFired;
		Ellipse _userPositionCircle;

		protected override async void OnElementChanged(ElementChangedEventArgs<Map> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				var mapModel = e.OldElement;
				MessagingCenter.Unsubscribe<Map, MapSpan>(this, "MapMoveToRegion");
				((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged -= OnCollectionChanged;
			}

			if (e.NewElement != null)
			{
				var mapModel = e.NewElement;

				if (Control == null)
				{
					SetNativeControl(new Bing.Maps.Map());
					Control.Credentials = FormsMaps.AuthenticationToken;
					Control.ViewChanged += (s, a) => UpdateVisibleRegion();
				}

				MessagingCenter.Subscribe<Map, MapSpan>(this, "MapMoveToRegion", (s, a) => MoveToRegion(a), mapModel);

				UpdateMapType();
				UpdateHasScrollEnabled();
				UpdateHasZoomEnabled();

				((ObservableCollection<Pin>)mapModel.Pins).CollectionChanged += OnCollectionChanged;

				if (mapModel.Pins.Any())
					LoadPins();

				await UpdateIsShowingUser();
			}
		}

		protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Map.MapTypeProperty.PropertyName)
				UpdateMapType();
			else if (e.PropertyName == Map.IsShowingUserProperty.PropertyName)
				await UpdateIsShowingUser();
			else if (e.PropertyName == Map.HasScrollEnabledProperty.PropertyName)
				UpdateHasScrollEnabled();
			else if (e.PropertyName == Map.HasZoomEnabledProperty.PropertyName)
				UpdateHasZoomEnabled();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;

				MessagingCenter.Unsubscribe<Map, MapSpan>(this, "MapMoveToRegion");

				if (Element != null)
					((ObservableCollection<Pin>)Element.Pins).CollectionChanged -= OnCollectionChanged;
			}
			base.Dispose(disposing);
		}

		async Task UpdateIsShowingUser()
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
			else if (_userPositionCircle != null && Control.Children.Contains(_userPositionCircle))
				Control.Children.Remove(_userPositionCircle);
		}

		void LoadPins()
		{
			foreach (var pin in Element.Pins)
				LoadPin(pin);
		}

		void ClearPins()
		{
			Control.Children.Clear();
#pragma warning disable 4014 // don't wanna block UI thread
			UpdateIsShowingUser();
#pragma warning restore
		}

		void RemovePin(Pin pinToRemove)
		{
			var pushPin = Control.Children.FirstOrDefault(c =>
			{
				var pin = (c as Pushpin);
				return (pin != null && pin.DataContext.Equals(pinToRemove));
			});

			if (pushPin != null)
				Control.Children.Remove(pushPin);
		}

		void LoadPin(Pin pin)
		{
			var pushPin = new Pushpin { Text = pin.Label, DataContext = pin };
			MapLayer.SetPosition(pushPin, new Location(pin.Position.Latitude, pin.Position.Longitude));
			pushPin.Tapped += (s, e) => ((s as Pushpin)?.DataContext as Pin)?.SendTap();
			Control.Children.Add(pushPin);
		}

		void UpdateMapType()
		{
			switch (Element.MapType)
			{
				case MapType.Street:
					Control.MapType = Bing.Maps.MapType.Road;
					break;
				case MapType.Satellite:
					Control.MapType = Bing.Maps.MapType.Aerial;
					break;
				case MapType.Hybrid:
					Control.MapType = Bing.Maps.MapType.Birdseye;
					break;
			}
		}

		void MoveToRegion(MapSpan span)
		{
			var center = new Location(span.Center.Latitude, span.Center.Longitude);
			var location = new LocationRect(center, span.LongitudeDegrees, span.LatitudeDegrees);
			Control.SetView(location);
		}

		void UpdateVisibleRegion()
		{
			if (Control == null || Element == null)
				return;

			if (!_firstZoomLevelChangeFired)
			{
				MoveToRegion(Element.LastMoveToRegion);
				_firstZoomLevelChangeFired = true;
				return;
			}

			var center = new Position(Control.Center.Latitude, Control.Center.Longitude);

			var boundingRegion = Control.Bounds;
			var result = new MapSpan(center, boundingRegion.Height, boundingRegion.Width);
			Element.SetVisibleRegion(result);
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
					ClearPins();
					break;
			}
		}

		void LoadUserPosition(Geocoordinate userCoordinate, bool center)
		{
			var userPosition = new Location
			{
				Latitude = userCoordinate.Point.Position.Latitude,
				Longitude = userCoordinate.Point.Position.Longitude
			};

			if (_userPositionCircle == null)
			{
				_userPositionCircle = new Ellipse
				{
					Stroke = new SolidColorBrush(Colors.White),
					Fill = new SolidColorBrush(Colors.Blue),
					StrokeThickness = 2,
					Height = 20,
					Width = 20,
					Opacity = 50
				};
			}

			if (Control.Children.Contains(_userPositionCircle))
				Control.Children.Remove(_userPositionCircle);

			MapLayer.SetPosition(_userPositionCircle, userPosition);
			MapLayer.SetPositionAnchor(_userPositionCircle, new Windows.Foundation.Point(0.5, 0.5));

			Control.Children.Add(_userPositionCircle);

			if (center)
			{
				Control.Center = userPosition;
				Control.ZoomLevel = 13;
			}
		}

		void UpdateHasZoomEnabled()
		{
		}

		void UpdateHasScrollEnabled()
		{
		}
	}
}