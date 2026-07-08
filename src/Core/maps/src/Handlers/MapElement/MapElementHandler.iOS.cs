using MapKit;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapElementHandler : ElementHandler<IMapElement, MKOverlayRenderer>
	{
		protected override MKOverlayRenderer CreatePlatformElement()
		{
			if (VirtualView.MapElementId != null)
			{
				var mapElementId = VirtualView.MapElementId;

				if (mapElementId is MKPolygon mKPolygon)
					return new MKPolygonRenderer(mKPolygon);

				if (mapElementId is MKPolyline line)
					return new MKPolylineRenderer(line);

				if (mapElementId is MKCircle circle)
					return new MKCircleRenderer(circle);
			}

			return new MKOverlayRenderer();
		}

		public static void MapStroke(IMapElementHandler handler, IMapElement mapElement)
		{
			var platformColor = mapElement.Stroke.ToColor()?.ToPlatform();
			if (handler.PlatformView is MKPolygonRenderer polygonRenderer)
				polygonRenderer.StrokeColor = platformColor;
			if (handler.PlatformView is MKPolylineRenderer polylineRenderer)
				polylineRenderer.StrokeColor = platformColor;
			if (handler.PlatformView is MKCircleRenderer circleRenderer)
				circleRenderer.StrokeColor = platformColor;
		}

		public static void MapStrokeThickness(IMapElementHandler handler, IMapElement mapElement)
		{
			if (handler.PlatformView is MKPolygonRenderer polygonRenderer)
				polygonRenderer.LineWidth = (nfloat)mapElement.StrokeThickness;
			if (handler.PlatformView is MKPolylineRenderer polylineRenderer)
				polylineRenderer.LineWidth = (nfloat)mapElement.StrokeThickness;
			if (handler.PlatformView is MKCircleRenderer circleRenderer)
				circleRenderer.LineWidth = (nfloat)mapElement.StrokeThickness;
		}

		public static void MapFill(IMapElementHandler handler, IMapElement mapElement)
		{
			if (mapElement is not IFilledMapElement filledMapElement)
				return;

			var platformColor = filledMapElement.Fill?.ToColor()?.ToPlatform();

			if (handler.PlatformView is MKPolygonRenderer polygonRenderer)
				polygonRenderer.FillColor = platformColor;
			if (handler.PlatformView is MKCircleRenderer circleRenderer)
				circleRenderer.FillColor = platformColor;
		}

		public static void MapIsVisible(IMapElementHandler handler, IMapElement mapElement)
		{
			handler.PlatformView.Alpha = mapElement.IsVisible ? 1 : 0;
		}

		public static void MapZIndex(IMapElementHandler handler, IMapElement mapElement)
		{
			// MapKit does not support fine-grained ZIndex on overlays.
			// Overlays are drawn in the order they are added to the map.
			// The property is accepted but has no visual effect on iOS/MacCatalyst.
		}
	}
}
