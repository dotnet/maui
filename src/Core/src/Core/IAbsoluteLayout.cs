using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui
{
	/// <summary>
	/// A Layout used to position and size children using explicit values.
	/// </summary>
	public interface IAbsoluteLayout : ILayout
	{
		/// <summary>
		/// Gets the layout bounds of a View.
		/// </summary>
		/// <param name="view">A visual element.</param>
		/// <returns>The layout bounds of the object.</returns>
		Rect GetLayoutBounds(IView view);

		/// <summary>
		/// Gets the layout flags that were specified when view was added to an AbsoluteLayout.
		/// </summary>
		/// <param name="view">A visual element.</param>
		/// <returns>The layout flags of the object.</returns>
		AbsoluteLayoutFlags GetLayoutFlags(IView view);
	}
}