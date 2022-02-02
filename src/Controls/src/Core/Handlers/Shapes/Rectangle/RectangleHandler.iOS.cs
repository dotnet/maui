using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class RectangleHandler : ViewHandler<Rectangle, MauiShapeView>
	{
		protected override MauiShapeView CreateNativeView() =>
			new MauiShapeView();

		public static void MapRadiusX(RectangleHandler handler, Rectangle rectangle)
		{
			handler.NativeView?.InvalidateShape(rectangle);
		}

		public static void MapRadiusY(RectangleHandler handler, Rectangle rectangle)
		{
			handler.NativeView?.InvalidateShape(rectangle);
		}
	}
}