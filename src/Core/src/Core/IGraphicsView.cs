using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IGraphicsView : IView
	{
		IDrawable Drawable { get; }
	}
}