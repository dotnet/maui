#nullable disable
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PathHandler
	{
		public static void MapShape(IShapeViewHandler handler, Path path)
		{
			handler.PlatformView?.UpdateShape(path);
		}

		public static void MapData(IShapeViewHandler handler, Path path)
		{
			handler.PlatformView?.UpdateShape(path);
		}

		public static void MapRenderTransform(IShapeViewHandler handler, Path path)
		{
			IDrawable drawable = handler.PlatformView?.Drawable;

			if (drawable == null)
				return;

			if (drawable is ShapeDrawable shapeDrawable)
			{
				Matrix? matrix = path.RenderTransform?.Value;

				if (matrix != null)
				{
					shapeDrawable.UpdateRenderTransform(matrix.Value.ToMatrix3X2());
				}
			}

			handler.PlatformView?.InvalidateShape(path);
		}
	}
}