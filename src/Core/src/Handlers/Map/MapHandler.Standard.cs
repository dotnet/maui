using System;

namespace Microsoft.Maui.Handlers
{
    public partial class MapHandler : ViewHandler<IMap, object>
	{

		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapMapType(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapIsShowingUser(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapHasScrollEnabled(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapHasTrafficEnabled(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapHasZoomEnabled(IMapHandler handler, IMap map) => throw new NotImplementedException();
	}
}
