using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public abstract class Geometry : BindableObject, IShape
	{
		public abstract PathF PathForBounds(Graphics.Rectangle rect);
	}
}