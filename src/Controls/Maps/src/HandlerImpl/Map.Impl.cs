using System;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class Map : IMap
	{
		//Maui.Maps.MapSpan _visibleArea;
		//Maui.Maps.MapSpan IMap.VisibleArea
		//{
		//	set
		//	{
		//		_visibleArea = value;
		//		SetVisibleRegion(new MapSpan(new Position(_visibleArea.Center.Latitude, _visibleArea.Center.Longitude), _visibleArea.LatitudeDegrees, _visibleArea.LongitudeDegrees));
		//	}
		//	get
		//	{
		//		if(LastMoveToRegion != null)
		//			return new Maui.Maps.MapSpan(new Devices.Sensors.Location(LastMoveToRegion.Center.Latitude, LastMoveToRegion.Center.Longitude), LastMoveToRegion.LatitudeDegrees, LastMoveToRegion.LongitudeDegrees);
		//		return new Maui.Maps.MapSpan(new Devices.Sensors.Location(VisibleRegion.Center.Latitude, VisibleRegion.Center.Longitude), VisibleRegion.LatitudeDegrees, VisibleRegion.LongitudeDegrees);
		//	}
		//}
	}
}
