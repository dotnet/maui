
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public sealed class Ellipse : Shape, IShape
	{
		public Ellipse() : base()
		{
			Aspect = Stretch.Fill;
		}

		public PathF PathForBounds(Graphics.Rectangle rect)
		{
			var path = new PathF();

			path.AppendEllipse(0f, 0f, (float)Width, (float)Height);

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}