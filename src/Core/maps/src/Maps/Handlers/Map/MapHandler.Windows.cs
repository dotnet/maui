using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
    public partial class MapHandler : ViewHandler<IMap, WebView2>
	{

		protected override WebView2 CreatePlatformView() => throw new NotImplementedException();

		public static void MapMapType(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapHasZoomEnabled(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapHasScrollEnabled(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapHasTrafficEnabled(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapIsShowingUser(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg) => throw new NotImplementedException();

		public static void MapPins(IMapHandler handler, IMap map) => throw new NotImplementedException();
	}
}
