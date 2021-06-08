using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Rectangle : IShape
	{
		public PathF PathForBounds(Graphics.Rectangle rect)
		{
			var path = new PathF();

			path.AppendRoundedRectangle(rect, (float)RadiusY + (float)RadiusX);

			return path;
		}
	}
}