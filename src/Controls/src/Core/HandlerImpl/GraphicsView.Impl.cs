using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class GraphicsView : View, IGraphicsView
	{
		public IDrawable Drawable { get; set; }
	}
}