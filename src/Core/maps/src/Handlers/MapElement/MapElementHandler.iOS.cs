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
				if (VirtualView is IGeoPathMapElement)
				{
					if (VirtualView is IFilledMapElement)
						return new MKPolygonRenderer((MKPolygon)VirtualView.MapElementId);
					else
						return new MKPolylineRenderer((MKPolyline)VirtualView.MapElementId);
				}
				if (VirtualView is ICircleMapElement)
					return new MKCircleRenderer((MKCircle)VirtualView.MapElementId);
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
	}
}
