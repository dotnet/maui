using System;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapHandler : ViewHandler<IMap, NotImplementedView>
	{
		protected override NotImplementedView CreatePlatformView() => new(nameof(IMap));

		[MissingMapper]
		public static void MapMapType(IMapHandler handler, IMap map) { }

		[MissingMapper]
		public static void MapIsShowingUser(IMapHandler handler, IMap map) { }

		[MissingMapper]
		public static void MapIsScrollEnabled(IMapHandler handler, IMap map) { }

		[MissingMapper]
		public static void MapIsTrafficEnabled(IMapHandler handler, IMap map) { }

		[MissingMapper]
		public static void MapIsZoomEnabled(IMapHandler handler, IMap map) { }

		[MissingMapper]
		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg) { }

		[MissingMapper]
		public static void MapPins(IMapHandler handler, IMap map) { }

		[MissingMapper]
		public static void MapElements(IMapHandler handler, IMap map) { }

		[MissingMapper]
		public void UpdateMapElement(IMapElement element) { }
	}
}