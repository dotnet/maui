using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public partial class MapHandler : ViewHandler<IMap, object>
	{

		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapMapType(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapIsShowingUser(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapHasScrollEnabled(IMapHandler handler, IMap map) => throw new NotImplementedException();


		public static void MapHasTrafficEnabled(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapHasZoomEnabled(IMapHandler handler, IMap map) => throw new NotImplementedException();

		public static void MapMoveToRegion(IMapHandler handler, IMap map, object? arg) => throw new NotImplementedException();
	}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
