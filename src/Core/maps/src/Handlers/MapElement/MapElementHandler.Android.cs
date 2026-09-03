using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapElementHandler : ElementHandler<IMapElement, Java.Lang.Object>
	{
		protected override Java.Lang.Object CreatePlatformElement()
		{
			if (VirtualView is IGeoPathMapElement geoPathMapElement)
			{
				if (geoPathMapElement is IFilledMapElement)
				{
					return new PolygonOptions();
				}
				else
				{
					return new PolylineOptions();
				}
			}

			if (VirtualView is ICircleMapElement circleMapElement)
				return new CircleOptions();

			return null!;
		}

		public static void MapStroke(IMapElementHandler handler, IMapElement mapElement)
		{
			if (mapElement.Stroke is not SolidPaint solidPaint)
				return;

			var platformColor = solidPaint.Color.AsColor();

			if (handler.PlatformView is PolygonOptions polygonOptions)
				polygonOptions.InvokeStrokeColor(platformColor);
			if (handler.PlatformView is PolylineOptions polyLineOptions)
				polyLineOptions.InvokeColor(platformColor);
			if (handler.PlatformView is CircleOptions circleOptions)
				circleOptions.InvokeStrokeColor(platformColor);
		}

		public static void MapStrokeThickness(IMapElementHandler handler, IMapElement mapElement)
		{
			if (handler.PlatformView is PolygonOptions polygonOptions)
				polygonOptions.InvokeStrokeWidth((float)mapElement.StrokeThickness);
			if (handler.PlatformView is PolylineOptions polyLineOptions)
				polyLineOptions.InvokeWidth((float)mapElement.StrokeThickness);
			if (handler.PlatformView is CircleOptions circleOptions)
				circleOptions.InvokeStrokeWidth((float)mapElement.StrokeThickness);
		}

		public static void MapFill(IMapElementHandler handler, IMapElement mapElement)
		{
			if (mapElement is not IFilledMapElement filledMapElement)
				return;

			if (filledMapElement.Fill is not SolidPaint solidPaintFill)
				return;

			var platformColor = solidPaintFill.Color.AsColor();
			if (handler.PlatformView is PolygonOptions polygonOptions)
				polygonOptions.InvokeFillColor(platformColor);
			if (handler.PlatformView is CircleOptions circleOptions)
				circleOptions.InvokeFillColor(platformColor);
		}

		public static void MapGeopath(IMapElementHandler handler, IMapElement mapElement)
		{
			if (mapElement is not IGeoPathMapElement geoPathMapElement)
				return;

			if (handler.PlatformView is PolygonOptions polygonOptions)
			{
				// Will throw an exception when added to the map if Points is empty
				if (geoPathMapElement.Count == 0)
				{
					polygonOptions.Points.Add(new LatLng(0, 0));
				}
				else
				{
					foreach (var position in geoPathMapElement)
					{
						polygonOptions.Points.Add(new LatLng(position.Latitude, position.Longitude));
					}
				}
			}

			if (handler.PlatformView is PolylineOptions polylineOptions)
			{
				// Will throw an exception when added to the map if Points is empty
				if (geoPathMapElement.Count == 0)
				{
					polylineOptions.Points.Add(new LatLng(0, 0));
				}
				else
				{
					foreach (var position in geoPathMapElement)
					{
						polylineOptions.Points.Add(new LatLng(position.Latitude, position.Longitude));
					}
				}
			}
		}
		public static void MapRadius(IMapElementHandler handler, IMapElement mapElement)
		{
			if (handler.PlatformView is CircleOptions circleOptions && mapElement is ICircleMapElement circleMapElement)
				circleOptions.InvokeRadius(circleMapElement.Radius.Meters);

		}
		public static void MapCenter(IMapElementHandler handler, IMapElement mapElement)
		{
			if (handler.PlatformView is CircleOptions circleOptions && mapElement is ICircleMapElement circleMapElement)
				circleOptions.InvokeCenter(new LatLng(circleMapElement.Center.Latitude, circleMapElement.Center.Longitude));

		}

		public static void MapIsVisible(IMapElementHandler handler, IMapElement mapElement)
		{
			// Visibility is applied on the native object after it is added to the map,
			// via the UpdateMapElement path in MapHandler.Android.cs.
			// PolygonOptions/PolylineOptions/CircleOptions don't expose a Visible setter.
		}

		public static void MapZIndex(IMapElementHandler handler, IMapElement mapElement)
		{
			// ZIndex is applied on the native object after it is added to the map,
			// via the UpdateMapElement path in MapHandler.Android.cs.
			// PolygonOptions/PolylineOptions/CircleOptions don't expose a ZIndex setter.
		}
	}
}
