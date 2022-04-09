using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui
{
	/// <summary>
	/// A Layout used to position and size children using explicit values.
	/// </summary>
	public interface IAbsoluteLayout : ILayout
	{
		Rect GetLayoutBounds(IView view);

		AbsoluteLayoutFlags GetLayoutFlags(IView view);
	}
}