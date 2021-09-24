using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public abstract class Geometry : BindableObject, IGeometry
	{
		public abstract void AppendPath(PathF path);

		PathF IShape.PathForBounds(Graphics.Rectangle bounds)
		{
			var path = new PathF();

			AppendPath(path);

			return path;
		}
	}
}