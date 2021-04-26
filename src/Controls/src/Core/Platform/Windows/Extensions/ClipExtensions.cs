using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.Maui.Controls.Shapes;
using WRectangleGeometry = Microsoft.UI.Xaml.Media.RectangleGeometry;
using WVector2 = System.Numerics.Vector2;


namespace Microsoft.Maui.Controls.Platform
{
	internal static class ClipExtensions
	{
		public static void Clip(this FrameworkElement frameworkElement, Geometry geometry)
		{
			var wGeometry = geometry.ToNative();

			if (wGeometry is WRectangleGeometry wRectangleGeometry && frameworkElement.Clip != wRectangleGeometry)
				frameworkElement.Clip = wRectangleGeometry;
		}

		public static void ClipVisual(this FrameworkElement frameworkElement, Geometry geometry)
		{
			var compositor = ElementCompositionPreview.GetElementVisual(frameworkElement).Compositor;
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
		}
	}
}