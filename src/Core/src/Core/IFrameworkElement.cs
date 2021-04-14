using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

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
		/// Gets the color which will fill the background of a FrameworkElement.
		/// </summary>
		Color BackgroundColor { get; }

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

		/// <summary>
		/// Id used by automation tools to interact with this FrameworkElement
		/// </summary>
		string AutomationId { get; }

		/// <summary>
		/// Direction in which the UI elements on the page are scanned by the eye
		/// </summary>
		FlowDirection FlowDirection { get; }

		/// <summary>
		/// Determines the horizontal aspect of this element's arrangement in a container
		/// </summary>
		LayoutAlignment HorizontalLayoutAlignment { get; }

		/// <summary>
		/// Determines the vertical aspect of this element's arrangement in a container
		/// </summary>
		LayoutAlignment VerticalLayoutAlignment { get; }

		/// <summary>
		/// Adds semantics to every FrameworkElement for accessibility
		/// </summary>
		Semantics Semantics { get; }
	}
}