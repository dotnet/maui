using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapPinHandler : ElementHandler<IMapPin, MarkerOptions>
	{
		// Keep track of the actual marker associated with this handler
		internal Marker? Marker { get; set; }

		protected override MarkerOptions CreatePlatformElement() => new MarkerOptions();

		public static void MapLocation(IMapPinHandler handler, IMapPin mapPin)
		{
			var mapPinHandler = handler as MapPinHandler;
			if (mapPin.Location != null)
			{
				// Always update the MarkerOptions
				var position = new LatLng(mapPin.Location.Latitude, mapPin.Location.Longitude);
				handler.PlatformView.SetPosition(position);

				// If we have a reference to the actual Marker on the map, update it too
				if (mapPinHandler?.Marker != null)
				{
					mapPinHandler.Marker.Position = position;
				}
			}
		}

		public static void MapLabel(IMapPinHandler handler, IMapPin mapPin)
		{
			handler.PlatformView.SetTitle(mapPin.Label);

			// Update the actual marker if available
			var mapPinHandler = handler as MapPinHandler;
			if (mapPinHandler?.Marker != null)
			{
				mapPinHandler.Marker.Title = mapPin.Label;
			}
		}

		public static void MapAddress(IMapPinHandler handler, IMapPin mapPin)
		{
			handler.PlatformView.SetSnippet(mapPin.Address);

			// Update the actual marker if available
			var mapPinHandler = handler as MapPinHandler;
			if (mapPinHandler?.Marker != null)
			{
				mapPinHandler.Marker.Snippet = mapPin.Address;
			}
		}
	}
}
