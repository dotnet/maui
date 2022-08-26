using Microsoft.Maui.Handlers;
namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapPinHandler : ElementHandler<IMapPin, ElmSharp.EvasObject>
	{
		protected override ElmSharp.EvasObject CreatePlatformElement() => throw new System.NotImplementedException();
		public static void MapLocation(IMapPinHandler handler, IMapPin mapPin) { }

		public static void MapLabel(IMapPinHandler handler, IMapPin mapPin) { }

		public static void MapAddress(IMapPinHandler handler, IMapPin mapPin) { }
	}
}
