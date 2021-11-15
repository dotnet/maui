using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IWindowOverlayElement : IDrawable
	{
		bool IsPointInElement(Point point);
	}
}
