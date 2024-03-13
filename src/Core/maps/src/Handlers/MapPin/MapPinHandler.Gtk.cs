using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapPinHandler : ElementHandler<IMapPin, object>
	{
		protected override object CreatePlatformElement() => new();

		[MissingMapper]
		public static void MapLocation(IMapPinHandler handler, IMapPin mapPin) { }

		[MissingMapper]
		public static void MapLabel(IMapPinHandler handler, IMapPin mapPin) { }

		[MissingMapper]
		public static void MapAddress(IMapPinHandler handler, IMapPin mapPin) { }
	}
}