using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a visual element that is used to place layouts and controls on the screen.
	/// </summary>
	public interface IView : IFrameworkElement, IArrangeable, IVisual, IFlowLayout 
		// TODO ezhart IWidget? IControl?
	{
		/// <summary>
		/// The Margin represents the distance between an view and its adjacent views.
		/// </summary>
		Thickness Margin { get; }

		/// <summary>
		/// Gets a value that determines whether this View should be part of the visual tree or not.
		/// </summary>
		Visibility Visibility { get; }
		
		/// <summary>
		/// Gets the bounds of the FrameworkElement.
		/// </summary>
		Rectangle Frame { get; }

		/// <summary>
		/// Gets the specified width of this FrameworkElement. 
		/// </summary>
		double Width { get; }

		/// <summary>
		/// Gets the specified height of this FrameworkElement. 
		/// </summary>
		double Height { get; }
		
		/// <summary>
		/// Gets the current desired Size of this FrameworkElement. 
		/// </summary>
		Size DesiredSize { get; }

		/// <summary>
		/// Determines the horizontal aspect of this element's arrangement in a container
		/// </summary>
		LayoutAlignment HorizontalLayoutAlignment { get; }

		/// <summary>
		/// Determines the vertical aspect of this element's arrangement in a container
		/// </summary>
		LayoutAlignment VerticalLayoutAlignment { get; }
	}
}