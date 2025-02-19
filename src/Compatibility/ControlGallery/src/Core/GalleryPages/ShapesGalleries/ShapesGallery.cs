using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.ShapesGalleries
{
	[Preserve(AllMembers = true)]
	public class ShapesGallery : ContentPage
	{
		public ShapesGallery()
		{
			Title = "Shapes Gallery";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						GalleryBuilder.NavButton("Ellipse Gallery", () => new EllipseGallery(), Navigation),
						GalleryBuilder.NavButton("Line Gallery", () => new LineGallery(), Navigation),
						GalleryBuilder.NavButton("Polygon Gallery", () => new PolygonGallery(), Navigation),
						GalleryBuilder.NavButton("Polyline Gallery", () => new PolylineGallery(), Navigation),
						GalleryBuilder.NavButton("Rectangle Gallery", () => new RectangleGallery(), Navigation),
						GalleryBuilder.NavButton("LineCap Gallery", () => new LineCapGallery(), Navigation),
						GalleryBuilder.NavButton("LineJoin Gallery", () => new LineJoinGallery(), Navigation),
						GalleryBuilder.NavButton("AutoSize Shapes Gallery", () => new AutoSizeShapesGallery(), Navigation),
						GalleryBuilder.NavButton("Path Gallery", () => new PathGallery(), Navigation),
						GalleryBuilder.NavButton("Path Aspect Gallery", () => new PathAspectGallery(), Navigation),
						GalleryBuilder.NavButton("Path LayoutOptions Gallery", () => new PathLayoutOptionsGallery(), Navigation),
						GalleryBuilder.NavButton("Transform Playground", () => new TransformPlaygroundGallery(), Navigation),
						GalleryBuilder.NavButton("Path Transform using string (TypeConverter) Gallery", () => new PathTransformStringGallery(), Navigation),
						GalleryBuilder.NavButton("Animate Shape Gallery", () => new AnimateShapeGallery(), Navigation),
						GalleryBuilder.NavButton("Shape AppThemes Gallery", () => new ShapeAppThemeGallery(), Navigation),
						GalleryBuilder.NavButton("Clip Gallery", () => new ClipGallery(), Navigation),
						GalleryBuilder.NavButton("Clip CornerRadius Gallery", () => new ClipCornerRadiusGallery(), Navigation),
						GalleryBuilder.NavButton("Clip Views Gallery", () => new ClipViewsGallery(), Navigation),
						GalleryBuilder.NavButton("Add/Remove Clip Gallery", () => new AddRemoveClipGallery(), Navigation),
						GalleryBuilder.NavButton("Clip Performance Gallery", () => new ClipPerformanceGallery(), Navigation)
					}
				}
			};
		}
	}
}