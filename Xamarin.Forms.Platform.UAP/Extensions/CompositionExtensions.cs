using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Xamarin.Forms.Shapes;
using WVector2 = System.Numerics.Vector2;

namespace Xamarin.Forms.Platform.UWP
{
	internal static class CompositionExtensions
	{
#if UWP_18362
		public static void Clip(this FrameworkElement frameworkElement, Geometry geometry)
		{
			var compositor = Window.Current.Compositor;
			var visual = ElementCompositionPreview.GetElementVisual(frameworkElement);

			CompositionClip compositionClip = null;

			if (geometry is EllipseGeometry ellipseGeometry)
			{
				var compositionEllipseGeometry = compositor.CreateEllipseGeometry();

				compositionEllipseGeometry.Center = new WVector2((float)ellipseGeometry.Center.X, (float)ellipseGeometry.Center.Y);
				compositionEllipseGeometry.Radius = new WVector2((float)ellipseGeometry.RadiusX, (float)ellipseGeometry.RadiusY);

				compositionClip = compositor.CreateGeometricClip(compositionEllipseGeometry);
			}
			else if (geometry is LineGeometry lineGeometry)
			{
				var compositionLineGeometry = compositor.CreateLineGeometry();

				compositionLineGeometry.Start = new WVector2((float)lineGeometry.StartPoint.X, (float)lineGeometry.StartPoint.Y);
				compositionLineGeometry.End = new WVector2((float)lineGeometry.EndPoint.X, (float)lineGeometry.EndPoint.Y);

				compositionClip = compositor.CreateGeometricClip(compositionLineGeometry);
			}
			else if (geometry is RectangleGeometry rectangleGeometry)
			{
				var compositionRectangleGeometry = compositor.CreateRectangleGeometry();

				compositionRectangleGeometry.Offset = new WVector2((float)rectangleGeometry.Rect.X, (float)rectangleGeometry.Rect.Y);
				compositionRectangleGeometry.Size = new WVector2((float)rectangleGeometry.Rect.Width, (float)rectangleGeometry.Rect.Height);

				compositionClip = compositor.CreateGeometricClip(compositionRectangleGeometry);
			}
	
			visual.Clip = compositionClip;
		}
#endif
	}
}