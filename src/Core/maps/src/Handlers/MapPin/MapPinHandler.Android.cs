using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapPinHandler : ElementHandler<IMapPin, MarkerOptions>
	{
		protected override MarkerOptions CreatePlatformElement() => new MarkerOptions();

		public static void MapLocation(IMapPinHandler handler, IMapPin mapPin)
		{
			if (mapPin.Location != null)
				handler.PlatformView.SetPosition(new LatLng(mapPin.Location.Latitude, mapPin.Location.Longitude));
		}

		public static void MapLabel(IMapPinHandler handler, IMapPin mapPin)
		{
			handler.PlatformView.SetTitle(mapPin.Label);
		}

		public static void MapAddress(IMapPinHandler handler, IMapPin mapPin)
		{
			handler.PlatformView.SetSnippet(mapPin.Address);
		}
	}
}
