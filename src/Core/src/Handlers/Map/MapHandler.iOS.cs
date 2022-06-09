using System;
using System.Collections.Generic;
using System.Text;
using MapKit;

namespace Microsoft.Maui.Handlers
{
	public partial class MapHandler : ViewHandler<IMap, MKMapView>
	{

		protected override MKMapView CreatePlatformView()
		{
			return new MKMapView();
		}

		public static void MapMapType(IMapHander handler, IMap map)
		{
			switch (map.MapType)
			{
				case MapType.Street:
					handler.PlatformView.MapType = MKMapType.Standard;
					break;
				case MapType.Satellite:
					handler.PlatformView.MapType = MKMapType.Satellite;
					break;
				case MapType.Hybrid:
					handler.PlatformView.MapType = MKMapType.Hybrid;
					break;
			}
		}
	}
}
