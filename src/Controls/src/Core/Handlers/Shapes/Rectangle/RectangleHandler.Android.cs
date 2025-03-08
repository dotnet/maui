#nullable disable
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class RectangleHandler
	{
		//TODO: Make this public on NET10
		internal static void MapBackground(IShapeViewHandler handler, Rectangle rectangle)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.ToPlatform().UpdateBackground(rectangle);
			handler.PlatformView?.InvalidateShape(rectangle);
		}

		public static void MapRadiusX(IShapeViewHandler handler, Rectangle rectangle)
		{
			handler.PlatformView?.InvalidateShape(rectangle);
		}

		public static void MapRadiusY(IShapeViewHandler handler, Rectangle rectangle)
		{
			handler.PlatformView?.InvalidateShape(rectangle);
		}
	}
}