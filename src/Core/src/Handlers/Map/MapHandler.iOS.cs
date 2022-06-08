using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class MapHandler : ViewHandler<IMap, MapKit.MKMapView>
	{

		protected override MapKit.MKMapView CreatePlatformView()
		{
			return new MapKit.MKMapView();
		}
	}
}
