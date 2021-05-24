using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// A View that occupies the entire screen.
	/// </summary>
	public interface IPage : IFrameworkElement, IArrangeable, IBackground, IOpacity, IFlowDirection
	{
		/// <summary>
		/// Gets the view that contains the content of the Page.
		/// </summary>
		public IView Content { get; }

		/// <summary>
		/// Gets the title of the Page.
		/// </summary>
		public string Title { get; }
	}

	public interface IArrangeable
	{
		/// <summary>
		/// Positions child elements and determines a size for an Element.
		/// </summary>
		/// <param name="bounds">The size that the parent computes for the child element.</param>
		/// <returns>Return the actual arranged Size for this element.</returns>
		Size Arrange(Rectangle bounds);

		/// <summary>
		/// Updates the size of an FrameworkElement.
		/// </summary>
		/// <param name="widthConstraint">The width that a parent element can allocate a child element.</param>
		/// <param name="heightConstraint">The height that a parent element can allocate a child element.</param>
		/// <returns>Return the desired Size for this element.</returns>
		Size Measure(double widthConstraint, double heightConstraint);

		/// <summary>
		/// Signals that the current measure value of this FrameworkElement is no longer valid and must be recomputed during the next measure pass.
		/// </summary>
		void InvalidateMeasure();

		/// <summary>
		/// Method that is called to invalidate the layout of this FrameworkElement.
		/// </summary>
		void InvalidateArrange();
	}

	public interface IBackground
	{
		/// <summary>
		/// Gets the paint which will fill the background of a FrameworkElement.
		/// </summary>
		Paint? Background { get; }
	}

	public interface IOpacity
	{
		/// <summary>
		/// Gets the opacity value applied to the view when it is rendered.
		/// </summary>
		double Opacity { get; }
	}

	public interface IFlowDirection 
	{
		/// <summary>
		/// Direction in which the UI elements on the page are scanned by the eye
		/// </summary>
		FlowDirection FlowDirection { get; }
	}
}