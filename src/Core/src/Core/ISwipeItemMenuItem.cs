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

		/// <summary>
		/// Gets the color used to tint the icon specified by <see cref="IImageSourcePart.Source"/>.
		/// </summary>
		/// <remarks>
		/// When <see langword="null"/>, a font icon is tinted with its own color (falling back to a
		/// color contrasting the item background), and image icons render with their original colors.
		/// When set, the color is applied to every icon type.
		/// </remarks>
#if NETSTANDARD2_0
		Color? IconColor { get; }
#else
		Color? IconColor => null;
#endif
	}
}
