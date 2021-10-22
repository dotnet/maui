using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui
{
	public interface IAbsoluteLayout : ILayout
	{
		Rectangle GetLayoutBounds(IView view);

		AbsoluteLayoutFlags GetLayoutFlags(IView view);
	}
}