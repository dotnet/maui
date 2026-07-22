using System.Collections.Generic;
using System.Linq;
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
