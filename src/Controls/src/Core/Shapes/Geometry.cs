using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public abstract class Geometry : BindableObject
	{
		//public abstract PathF PathForBounds(Graphics.Rectangle rect);
		public abstract void AppendToPath(PathF path);
	}
}