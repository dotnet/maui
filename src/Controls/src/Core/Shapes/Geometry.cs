using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public abstract class Geometry : BindableObject
	{
		public abstract void AppendPath(PathF path);
	}
}