using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Xamarin.Forms.Shapes;
using WRectangleGeometry = Windows.UI.Xaml.Media.RectangleGeometry;

#if UWP_18362
using WVector2 = System.Numerics.Vector2;
#endif

namespace Xamarin.Forms.Platform.UWP
{
	internal static class ClipExtensions
	{
		public static void Clip(this FrameworkElement frameworkElement, Geometry geometry)
		{
			var wGeometry = geometry.ToWindows();

			if (wGeometry is WRectangleGeometry wRectangleGeometry && frameworkElement.Clip != wRectangleGeometry)
				frameworkElement.Clip = wRectangleGeometry;
		}

		public static void ClipVisual(this FrameworkElement frameworkElement, Geometry geometry)
		{
			// UIElement.Clip only support rectangle geometry to be used for clipping area sizing.
			// If the used Build is 17763 or higher, we use Composition's APIs (CompositionGeometricClip) to allow Clip complex geometries.
#if UWP_18362
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

			if (visual.Clip != compositionClip)
				visual.Clip = compositionClip;
#endif
		}
	}
}