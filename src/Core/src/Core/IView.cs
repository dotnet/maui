#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a visual element that is used to place layouts and controls on the screen.
	/// </summary>
	public interface IView : IElement, ITransform
	{
		/// <summary>
		/// Id used by automation tools to interact with this View
		/// </summary>
		string AutomationId { get; }

		/// <summary>
		/// Direction in which the UI elements are scanned by the eye
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
		/// Adds semantics to every View for accessibility
		/// </summary>
		Semantics? Semantics { get; }

		/// <summary>
		/// Gets the Path used to define the outline of the contents of a View.
		/// </summary>
		IShape? Clip { get; }

		/// <summary>
		/// Paints a shadow around the target View.
		/// </summary>
		IShadow? Shadow { get; }

		/// <summary>
		/// Gets a value indicating whether this View is enabled in the user interface. 
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Gets a value indicating whether this View is focused currently.
		/// </summary>
		bool IsFocused { get; set; }

		/// <summary>
		/// Gets a value that determines whether this View should be part of the visual tree or not.
		/// </summary>
		Visibility Visibility { get; }

		/// <summary>
		/// Gets a value that indicates if the view is constrained to a fixed size in either or both the horizontal and vertical directions.
		/// </summary>
		SizeConstraint SizeConstraint { get; }

		/// <summary>
		/// Gets the opacity value applied to the view when it is rendered.
		/// </summary>
		double Opacity { get; }

		/// <summary>
		/// Gets the paint which will fill the background of a View.
		/// </summary>
		Paint? Background { get; }

		/// <summary>
		/// Gets the bounds of the View within its container.
		/// </summary>
		Rect Frame { get; set; }

		/// <summary>
		/// Gets the specified width of the IView. 
		/// </summary>
		double Width { get; }

		/// <summary>
		/// Gets the specified minimum width constraint of the IView, between zero and double.PositiveInfinity.
		/// </summary>
		double MinimumWidth { get; }

		/// <summary>
		/// Gets the specified maximum width constraint of the IView, between zero and double.PositiveInfinity.
		/// </summary>
		double MaximumWidth { get; }

		/// <summary>
		/// Gets the specified height of the IView. 
		/// </summary>
		double Height { get; }

		/// <summary>
		/// Gets the specified minimum height constraint of the IView, between zero and double.PositiveInfinity.
		/// </summary>
		double MinimumHeight { get; }

		/// <summary>
		/// Gets the specified maximum height constraint of the IView, between zero and double.PositiveInfinity.
		/// </summary>
		double MaximumHeight { get; }

		/// <summary>
		/// The Margin represents the distance between an view and its adjacent views.
		/// </summary>
		Thickness Margin { get; }

		/// <summary>
		/// Gets the current desired Size of this View. 
		/// </summary>
		Size DesiredSize { get; }

		/// <summary>
		/// Determines the drawing order of this IView within an ILayout; higher z-indexes will draw over lower z-indexes.
		/// </summary>
		int ZIndex { get; }

		/// <summary>
		/// Gets or sets the View Handler of the View.
		/// </summary>
		new IViewHandler? Handler { get; set; }

		/// <summary>
		/// Positions child elements and determines a size for an Element.
		/// </summary>
		/// <param name="bounds">The size that the parent computes for the child element.</param>
		/// <returns>Return the actual arranged Size for this element.</returns>
		Size Arrange(Rect bounds);

		/// <summary>
		/// Updates the size of an View.
		/// </summary>
		/// <param name="widthConstraint">The width that a parent element can allocate a child element.</param>
		/// <param name="heightConstraint">The height that a parent element can allocate a child element.</param>
		/// <returns>Return the desired Size for this element.</returns>
		Size Measure(double widthConstraint, double heightConstraint);

		/// <summary>
		/// Signals that the current measure value of this View is no longer valid and must be recomputed during the next measure pass.
		/// </summary>
		void InvalidateMeasure();

		/// <summary>
		/// Method that is called to invalidate the layout of this View.
		/// </summary>
		void InvalidateArrange();

		/// <summary>
		/// Attempts to set focus to this View.
		/// </summary>
		/// <returns>true if the keyboard focus was set to this element; false if the call to this method did not force a focus change.</returns>
		bool Focus();

		/// <summary>
		/// Unsets focus to this View.
		/// </summary>
		void Unfocus();

		/// <summary>
		/// Gets a value indicating whether this element should be involved in the user interaction cycle.
		/// </summary>
		bool InputTransparent { get; }
	}
}