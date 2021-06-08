using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Line : IShape
	{
		public PathF PathForBounds(Graphics.Rectangle rect)
		{
			var path = new PathF();

			path.MoveTo((float)X1, (float)Y1);
			path.LineTo((float)X2, (float)Y2);

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}