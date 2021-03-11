using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a framework-level set of properties, events, and methods for .NET MAUI elements. 
	/// </summary>
	public interface IFrameworkElement
	{
		/// <summary>
		/// Gets a value indicating whether this FrameworkElement is enabled in the user interface. 
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Gets the brush which will fill the background of a FrameworkElement.
		/// </summary>
		IBrush Background { get; }

		/// <summary>
		/// Gets the bounds of the FrameworkElement.
		/// </summary>
		Rectangle Frame { get; }

		/// <summary>
		/// Gets the current rendered width of this FrameworkElement. 
		/// </summary>
		double Width { get; }

		/// <summary>
		/// Gets the current rendered height of this FrameworkElement. 
		/// </summary>
		double Height { get; }

		/// <summary>
		/// Gets or sets the View Handler of the FrameworkElement.
		/// </summary>
		IViewHandler? Handler { get; set; }

		/// <summary>
		/// Gets the Parent of the Element.
		/// </summary>
		IFrameworkElement? Parent { get; }

		/// <summary>
		/// Positions child elements and determines a size for an Element.
		/// </summary>
		/// <param name="bounds">The size that the parent computes for the child element.</param>
		void Arrange(Rectangle bounds);

		/// <summary>
		/// Updates the size of an FrameworkElement.
		/// </summary>
		/// <param name="widthConstraint">The width that a parent element can allocate a child element.</param>
		/// <param name="heightConstraint">The height that a parent element can allocate a child element.</param>
		/// <returns>Return the rendered Size for this element.</returns>
		Size Measure(double widthConstraint, double heightConstraint);

		/// <summary>
		/// Gets the current rendered Size of this FrameworkElement. 
		/// </summary>
		Size DesiredSize { get; }

		/// <summary>
		/// Gets a value indicating whether the current size returned by layout measure is valid.
		/// </summary>
		bool IsMeasureValid { get; }

		/// <summary>
		/// Gets a value indicating whether the computed size and position of child elements in this element's layout are valid.
		/// </summary>
		bool IsArrangeValid { get; }

		/// <summary>
		/// Gets a value indicating whether the computed size and position of child elements in this element's layout are valid.
		/// </summary>
		void InvalidateMeasure();

		/// <summary>
		/// Method that is called to invalidate the layout of this FrameworkElement.
		/// </summary>
		void InvalidateArrange();
	}
}