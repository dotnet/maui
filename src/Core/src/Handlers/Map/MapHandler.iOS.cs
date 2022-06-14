using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CoreLocation;
using MapKit;
using Microsoft.Maui.Core;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Handlers
{
	public partial class MapHandler : ViewHandler<IMap, MKMapView>
	{
		CLLocationManager? _locationManager;

		protected override MKMapView CreatePlatformView()
		{
			return new MKMapView();
		}

		protected override void ConnectHandler(MKMapView platformView)
		{
			base.ConnectHandler(platformView);
			_locationManager = new CLLocationManager();

			var mapsPinsItemsSource = (ObservableCollection<IMapPin>)VirtualView.Pins;
			mapsPinsItemsSource.CollectionChanged += OnPinCollectionChanged;
			OnPinCollectionChanged(mapsPinsItemsSource, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void DisconnectHandler(MKMapView platformView)
		{
			base.DisconnectHandler(platformView);

			var mapsPinsItemsSource = (ObservableCollection<IMapPin>)VirtualView.Pins;
			mapsPinsItemsSource.CollectionChanged -= OnPinCollectionChanged;

			foreach (IMapPin pin in mapsPinsItemsSource)
			{
				pin.PropertyChanged -= PinOnPropertyChanged;
			}
		}

		void OnPinCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			Dispatcher.GetForCurrentThread()?.Dispatch(() => PinCollectionChanged(e));
		}

		void PinCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddPins(e.NewItems ?? new List<IMapPin>());
					break;
				case NotifyCollectionChangedAction.Remove:
					RemovePins(e.OldItems ?? new List<IMapPin>());
					break;
				case NotifyCollectionChangedAction.Replace:
					RemovePins(e.OldItems ?? new List<IMapPin>());
					AddPins(e.NewItems ?? new List<IMapPin>());
					break;
				case NotifyCollectionChangedAction.Reset:
					if (PlatformView.Annotations?.Length > 0)
						PlatformView.RemoveAnnotations(PlatformView.Annotations);
					AddPins((IList)VirtualView.Pins);
					break;
				case NotifyCollectionChangedAction.Move:
					//do nothing
					break;
			}
		}

		void AddPins(IList pins)
		{
			foreach (IMapPin pin in pins)
			{
				pin.PropertyChanged += PinOnPropertyChanged;

				var annotation = CreateAnnotation(pin);
				pin.MarkerId = annotation;
				PlatformView.AddAnnotation(annotation);
			}
		}

		void RemovePins(IList pins)
		{
			foreach (IMapPin pin in pins)
			{
				pin.PropertyChanged -= PinOnPropertyChanged;
				PlatformView.RemoveAnnotation((IMKAnnotation)pin.MarkerId);
			}
		}

		IMKAnnotation CreateAnnotation(IMapPin pin)
		{
			return new MKPointAnnotation
			{
				Title = pin.Label,
				Subtitle = pin.Address ?? "",
				Coordinate = new CLLocationCoordinate2D(pin.Position.Latitude, pin.Position.Longitude)
			};
		}

		void PinOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			IMapPin pin = sender as IMapPin ?? throw new ArgumentNullException(nameof(sender), $"Argument cannot be null for {nameof(PinCollectionChanged)}");
			var annotation = pin.MarkerId as MKPointAnnotation;

			if (annotation == null)
			{
				return;
			}

			if (e.PropertyName == nameof(IMapPin.Label))
			{
				annotation.Title = pin.Label;
			}
			else if (e.PropertyName == nameof(IMapPin.Address))
			{
				annotation.Subtitle = pin.Address;
			}
			else if (e.PropertyName == nameof(IMapPin.Position))
			{
				annotation.Coordinate = new CLLocationCoordinate2D(pin.Position.Latitude, pin.Position.Longitude);
			}

		}

		public static void MapMapType(IMapHandler handler, IMap map)
		{
			switch (map.MapType)
			{
				case MapType.Street:
					handler.PlatformView.MapType = MKMapType.Standard;
					break;
				case MapType.Satellite:
					handler.PlatformView.MapType = MKMapType.Satellite;
					break;
				case MapType.Hybrid:
					handler.PlatformView.MapType = MKMapType.Hybrid;
					break;
			}
		}

		public static void MapIsShowingUser(IMapHandler handler, IMap map)
		{

#if !MACCATALYST
			if (map.IsShowingUser)
			{
				MapHandler? mapHandler = handler as MapHandler;
				mapHandler?._locationManager?.RequestWhenInUseAuthorization();
			}
#endif
			handler.PlatformView.ShowsUserLocation = map.IsShowingUser;
		}

		public static void MapHasScrollEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ScrollEnabled = map.HasScrollEnabled;
		}

		public static void MapHasTrafficEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ShowsTraffic = map.HasTrafficEnabled;
		}

		public static void MapHasZoomEnabled(IMapHandler handler, IMap map)
		{
			handler.PlatformView.ZoomEnabled = map.HasZoomEnabled;
		}
	}
}
