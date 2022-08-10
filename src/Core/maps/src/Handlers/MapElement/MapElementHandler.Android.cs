using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapElementHandler : ElementHandler<IMapElement, MarkerOptions>
	{
		protected override MarkerOptions CreatePlatformElement() => new MarkerOptions();

		public static void MapStroke(IMapElementHandler handler, IMapElement mapElement) => throw new System.NotImplementedException();

		public static void MapStrokeThickness(IMapElementHandler handler, IMapElement mapElement) => throw new System.NotImplementedException();

		public static void MapFill(IMapElementHandler handler, IMapElement mapElement) => throw new System.NotImplementedException();

		//public static void MapPosition(IMapPinHandler handler, IMapPin mapPin)
		//{
		//	handler.PlatformView.SetPosition(new LatLng(mapPin.Position.Latitude, mapPin.Position.Longitude));
		//}

		//public static void MapLabel(IMapPinHandler handler, IMapPin mapPin)
		//{
		//	handler.PlatformView.SetTitle(mapPin.Label);
		//}

		//public static void MapAddress(IMapPinHandler handler, IMapPin mapPin)
		//{
		//	handler.PlatformView.SetSnippet(mapPin.Address);
		//}
	}
}
