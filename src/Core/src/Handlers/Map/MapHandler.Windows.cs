using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
    public partial class MapHandler : ViewHandler<IMap, WebView2>
	{

		protected override WebView2 CreatePlatformView() => throw new NotImplementedException();

		public static void MapMapType(IMapHander handler, IMap map) => throw new NotImplementedException();

		public static void MapHasZoomEnabled(IMapHander handler, IMap map) => throw new NotImplementedException();

		public static void MapHasScrollEnabled(IMapHander handler, IMap map) => throw new NotImplementedException();

		public static void MapHasTrafficEnabled(IMapHander handler, IMap map) => throw new NotImplementedException();

		public static void MapIsShowingUser(IMapHander handler, IMap map) => throw new NotImplementedException();
	}
}
