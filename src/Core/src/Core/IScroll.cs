using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// A View capable of scrolling if its Content requires.
	/// </summary>
	public interface IScroll : IView
	{
		/// <summary>
		/// Gets the content of the scroll.
		/// </summary>
		IView Content { get; }

		/// <summary>
		/// Gets the scrolling directions.
		/// </summary>
		ScrollOrientation Orientation { get; }

		/// <summary>
		/// Gets the size of the Content.
		/// </summary>
		SizeF ContentSize { get; }

		/// <summary>
		/// Gets a value that controls when the horizontal scroll bar is visible.
		/// </summary>
		ScrollBarVisibility HorizontalScrollBarVisibility { get; }

		/// <summary>
		/// Gets a value that controls when the vertical scroll bar is visible.
		/// </summary>
		ScrollBarVisibility VerticalScrollBarVisibility { get; }
	}
}