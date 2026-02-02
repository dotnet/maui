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

		// Note: ImageSource is handled in MapHandler.AddPinAsync
		// because the icon must be set on MarkerOptions BEFORE calling Map.AddMarker()
		public static void MapImageSource(IMapPinHandler handler, IMapPin mapPin)
		{
			// No-op: Image is applied when the marker is created in MapHandler.AddPinAsync
		}
	}
}
