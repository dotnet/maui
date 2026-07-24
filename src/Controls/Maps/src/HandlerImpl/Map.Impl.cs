using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Map : IMap, IEnumerable<IMapPin>
	{
		IList<IMapElement> IMap.Elements => _mapElements.Cast<IMapElement>().ToList();

		IList<IMapPin> IMap.Pins => _pins.Cast<IMapPin>().ToList();

		Location? IMap.LastUserLocation => _lastUserLocation;

		void IMap.Clicked(Location location) => MapClicked?.Invoke(this, new MapClickedEventArgs(location));

		bool IMap.ClusterClicked(IReadOnlyList<IMapPin> pins, Location location)
		{
			// Convert IMapPin to Pin for the event args
			var controlPins = pins.OfType<Pin>().ToList();
			var args = new ClusterClickedEventArgs(controlPins, location);
			ClusterClicked?.Invoke(this, args);
			return args.Handled;
		}

		Microsoft.Maui.IImageSource? IMap.GetClusterImage(IReadOnlyList<IMapPin> pins, int count, Location location)
		{
			var provider = ClusterImageProvider;
			if (provider is not null)
			{
				// The provider is app code invoked from platform callbacks (a native MapKit delegate on
				// iOS, a fire-and-forget task on Android) where an unhandled exception either crashes the
				// app or silently drops the cluster marker - degrade to the static/default icon instead.
				try
				{
					// Pins/identifier are resolved lazily: a provider that only reads Count (like the
					// sample) never triggers the platform's O(members × pins) resolution scan.
					var image = provider(new ClusterInfo(count, location,
						() => pins.OfType<Pin>().ToList(),
						() =>
						{
							foreach (var pin in pins)
								if (pin is Pin controlPin)
									return controlPin.ClusteringIdentifier ?? Pin.DefaultClusteringIdentifier;
							return Pin.DefaultClusteringIdentifier;
						}));
					if (image is not null)
						return image;
				}
				catch (Exception ex)
				{
					Handler?.MauiContext?.Services?.GetService<ILogger<Map>>()?.LogWarning(ex, "ClusterImageProvider threw; falling back to the static or default cluster icon");
				}
			}

			return ClusterImageSource;
		}

		void IMap.UserLocationUpdated(Location location)
		{
			if (Equals(_lastUserLocation, location))
				return;

			OnPropertyChanging(nameof(LastUserLocation));
			_lastUserLocation = location;
			OnPropertyChanged(nameof(LastUserLocation));
			UserLocationChanged?.Invoke(this, new UserLocationChangedEventArgs(location));
		}

		void IMap.LongClicked(Location location) => MapLongClicked?.Invoke(this, new MapClickedEventArgs(location));

		void IMap.ShowInfoWindow(IMapPin pin) => Handler?.Invoke(nameof(IMap.ShowInfoWindow), pin);

		void IMap.HideInfoWindow(IMapPin pin) => Handler?.Invoke(nameof(IMap.HideInfoWindow), pin);

		void IMap.MoveToRegion(MapSpan region) => MoveToRegion(region);

		void IMap.MoveToRegion(MapSpan region, bool animated) => MoveToRegion(region, animated);

		MapSpan? IMap.VisibleRegion
		{
			get
			{
				return _visibleRegion;
			}
			set
			{
				SetVisibleRegion(value);
			}

		}

		/// <summary>
		/// Raised when the handler for this map control changed.
		/// </summary>
		protected override void OnHandlerChanged()
		{
			base.OnHandlerChanged();
			//The user specified on the ctor a MapSpan we now need the handler to move to that region
			Handler?.Invoke(nameof(IMap.MoveToRegion), new MoveToRegionRequest(_lastMoveToRegion, false));
		}

	}
}
