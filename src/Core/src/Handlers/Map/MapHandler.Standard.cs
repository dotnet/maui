using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
    public partial class MapHandler : ViewHandler<IMap, object>
	{

		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapMapType(IMapHander handler, IMap map) => throw new NotImplementedException();

		public static void MapIsShowingUser(IMapHander handler, IMap map) => throw new NotImplementedException();
	}
}
