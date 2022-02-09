using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface ISwipeItemMenuItem : IMenuElement, ISwipeItem
	{
		/// <summary>
		/// Gets the paint which will fill the background of a View.
		/// </summary>
		Paint? Background { get; }

		/// <summary>
		/// Gets a value that determines whether this View should be part of the visual tree or not.
		/// </summary>
		Visibility Visibility { get; }
	}
}
