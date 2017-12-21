using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.WPF;
using Microsoft.Maps.MapControl.WPF;
using WMap = Microsoft.Maps.MapControl.WPF.Map;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Device.Location;
using System.Windows.Threading;

namespace Xamarin.Forms.Maps.WPF
{
	public class MapRenderer : ViewRenderer<Map, WMap>
	{
		DispatcherTimer _timer;

		protected override async void OnElementChanged(ElementChangedEventArgs<Map> e)
		{
			if (e.OldElement != null) // Clear old element event
			{
				MessagingCenter.Unsubscribe<Map, MapSpan>(this, "MapMoveToRegion");
				((ObservableCollection<Pin>)e.OldElement.Pins).CollectionChanged -= OnCollectionChanged;
			}

			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new WMap());
					Control.CredentialsProvider = new ApplicationIdCredentialsProvider(FormsMaps.AuthenticationToken);
					Control.ViewChangeOnFrame += Control_ViewChangeOnFrame;
				}

				// Update control property 
				UpdateMapType();
				await UpdateIsShowingUser();
				UpdateHasZoomEnabled();
				UpdateHasZoomEnabled();
				UpdateVisibleRegion();

				// Suscribe element event
				MessagingCenter.Subscribe<Map, MapSpan>(this, "MapMoveToRegion", (s, a) => MoveToRegion(a), Element);
				((ObservableCollection<Pin>)Element.Pins).CollectionChanged += OnCollectionChanged;
			}

			base.OnElementChanged(e);
		}

		void Control_ViewChangeOnFrame(object sender, MapEventArgs e)
		{
			UpdateVisibleRegion();
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

		void UpdateVisibleRegion()
		{
			if (Control == null || Element == null)
				return;

			try
			{
				var boundingBox = Control.BoundingRectangle;

				if (boundingBox != null)
				{
					var center = new Position(boundingBox.Center.Latitude, boundingBox.Center.Longitude);
					var latitudeDelta = Math.Abs(boundingBox.Northwest.Latitude - boundingBox.Southeast.Latitude);
					var longitudeDelta = Math.Abs(boundingBox.Northwest.Longitude - boundingBox.Southeast.Longitude);
					Element.SetVisibleRegion(new MapSpan(center, latitudeDelta, longitudeDelta));
				}
			}
			catch (Exception)
			{
			}
		}

		async Task UpdateIsShowingUser(bool moveToLocation = true)
		{
			if (Control == null || Element == null) return;

			if (Element.IsShowingUser)
			{
				var location = await GetCurrentLocation();
				if(location != null)
					LoadUserPosition(location, moveToLocation);

				if (Control == null || Element == null) return;

				if (_timer == null)
				{
					_timer = new DispatcherTimer();
					_timer.Tick += async (s, o) => await UpdateIsShowingUser(moveToLocation: false);
					_timer.Interval = TimeSpan.FromSeconds(15);
				}

				if (!_timer.IsEnabled)
					_timer.Start();
			}
			else
			{
				_timer?.Stop();
				
				if (Control.Children.Contains(_userPositionPin))
					Control.Children.Remove(_userPositionPin);
			}
		}

		void LoadPins()
		{
			foreach (var pin in Element.Pins)
				LoadPin(pin);
		}

		void ClearPins()
		{
			Control.Children.Clear();
		}

		void RemovePin(Pin pinToRemove)
		{
			var pushPin = Control.Children.Cast<FormsPushPin>().FirstOrDefault(x => x.Pin == pinToRemove);
			
			if (pushPin != null)
				Control.Children.Remove(pushPin);
		}

		void LoadPin(Pin pin)
		{
			Control.Children.Add(new FormsPushPin(pin));
		}

		void UpdateMapType()
		{
			switch (Element.MapType)
			{
				case MapType.Street:
					Control.Mode = new RoadMode();
					break;
				case MapType.Satellite:
					Control.Mode = new AerialMode();
					break;
				case MapType.Hybrid:
					Control.Mode = new AerialMode(true);
					break;
			}
		}

		void MoveToRegion(MapSpan span)
		{
			var nw = new Location
			{
				Latitude = span.Center.Latitude + span.LatitudeDegrees / 2,
				Longitude = span.Center.Longitude - span.LongitudeDegrees / 2
			};
			var se = new Location
			{
				Latitude = span.Center.Latitude - span.LatitudeDegrees / 2,
				Longitude = span.Center.Longitude + span.LongitudeDegrees / 2
			};

			Control.SetView(new LocationRect(nw, se));
		}

		FormsPushPin _userPositionPin;

		void LoadUserPosition(GeoCoordinate userCoordinate, bool center)
		{
			if (Control == null || Element == null) return;

			var userPosition = new Location
			{
				Latitude = userCoordinate.Latitude,
				Longitude = userCoordinate.Longitude
			};
			
			if (Control.Children.Contains(_userPositionPin))
				Control.Children.Remove(_userPositionPin);
			
			_userPositionPin = new FormsPushPin(new Pin() { Position = new Position(userCoordinate.Latitude, userCoordinate.Longitude) });

			Control.Children.Add(_userPositionPin);

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

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				_timer?.Stop();
				_timer = null;

				if (Control != null)
				{
					Control.ViewChangeOnFrame -= Control_ViewChangeOnFrame;
				}

				if (Element != null)
				{
					MessagingCenter.Unsubscribe<Map, MapSpan>(this, "MapMoveToRegion");
					((ObservableCollection<Pin>)Element.Pins).CollectionChanged -= OnCollectionChanged;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}


		/* Tools */
		Task<GeoCoordinate> GetCurrentLocation()
		{
			TaskCompletionSource<GeoCoordinate> taskCompletionSource = new TaskCompletionSource<GeoCoordinate>();

			GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
			watcher.StatusChanged += (sender, e) =>
			{
				switch (e.Status)
				{
					case GeoPositionStatus.Disabled:
						watcher.Stop();
						taskCompletionSource.SetResult(null);
						break;
				}
			};

			watcher.PositionChanged += (sender, e) =>
			{
				watcher.Stop();
				taskCompletionSource.SetResult(e.Position.Location);
			};
			watcher.Start();

			return taskCompletionSource.Task;
		}
	}
}
