using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Ellipse : IShape
	{
		public PathF PathForBounds(Graphics.Rectangle rect)
		{
			var path = new PathF();

			path.AppendEllipse(0f, 0f, (float)Width, (float)Height);

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}