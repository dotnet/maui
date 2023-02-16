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
		/// Gets the layout bounds of an IView.
		/// </summary>
		/// <param name="view">A visual element.</param>
		/// <returns>The layout bounds of the object.</returns>
		Rect GetLayoutBounds(IView view);

		/// <summary>
		/// Gets the layout flags of the IView in the IAbsoluteLayout.
		/// </summary>
		/// <param name="view">A visual element.</param>
		/// <returns>The layout flags of the object.</returns>
		AbsoluteLayoutFlags GetLayoutFlags(IView view);
	}
}