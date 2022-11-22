using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui
{
	/// <summary>
	/// A Flexbox-like layout that lays out child elements in optionally wrappable rows or columns of 
	/// child elements.
	/// </summary>
	public interface IFlexLayout : ILayout
	{
		/// <summary>
		/// Gets the flex direction for child elements within this layout.
		/// </summary>
		FlexDirection Direction { get; }

		/// <summary>
		/// Gets a value that that describes how child elements are justified when there is extra space around them.
		/// </summary>
		FlexJustify JustifyContent { get; }

		/// <summary>
		/// Gets a value that controls how multiple rows or columns of child elements are aligned.
		/// </summary>
		FlexAlignContent AlignContent { get; }

		/// <summary>
		/// Gets a value that controls how child elements are laid out within their row or column.
		/// </summary>
		FlexAlignItems AlignItems { get; }

		/// <summary>
		/// Gets a value that controls whether the coordinates of child elements are specified in absolute or relative terms.
		/// </summary>
		FlexPosition Position { get; }

		/// <summary>
		/// Gets a value that controls whether and how child elements within this layout wrap.
		/// </summary>
		FlexWrap Wrap { get; }

		/// <summary>
		/// Returns the visual order of the element among its siblings.
		/// </summary>
		/// <param name="view">The view for which to retrieve the property value.</param>
		/// <returns>The visual order of the element among its siblings.</returns>
		int GetOrder(IView view);

		/// <summary>
		/// Returns the value that determines the proportional growth that this element will accept to accommodate 
		/// the layout in the row or column.
		/// </summary>
		/// <param name="view">The view for which to retrieve the property value.</param>
		/// <returns>The value that determines the proportional growth that this element will accept to  the layout in the row or column.</returns>
		float GetGrow(IView view);

		/// <summary>
		/// Returns the value that determines the proportional reduction in size that this element will accept to  the layout in the row or column.
		/// </summary>
		/// <param name="view">The view for which to retrieve the property value.</param>
		/// <returns>The proportional reduction in size that this element will accept to  the layout in the row or column.</returns>
		float GetShrink(IView view);

		/// <summary>
		/// Returns the value that optionally overrides the item alignment for this child within its row or column in the parent.
		/// </summary>
		/// <param name="view">The view for which to retrieve the property value</param>
		/// <returns>The value that optionally overrides the item alignment for this child within its row or column in the parent.</returns>
		FlexAlignSelf GetAlignSelf(IView view);

		/// <summary>
		/// Returns the value that describes this element's relative or absolute basis length.
		/// </summary>
		/// <param name="view">The view for which to retrieve the property value.</param>
		/// <returns>The value that describes this element's relative or absolute basis length.</returns>
		FlexBasis GetBasis(IView view);

		Rect GetFlexFrame(IView view);

		void Layout(double width, double height);
	}
}