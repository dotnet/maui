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
		/// When <see langword="null"/>, a font icon is tinted by its own color, then by the item's
		/// <see cref="ITextStyle.TextColor"/>, then by a color contrasting the item background; image
		/// icons render with their original colors. When set, the color tints font icons and image icons
		/// on Android, iOS, and MacCatalyst (including stream-based sources, since the resolved platform
		/// image is tinted), and font, file-based, and URI-based icons on Windows. Stream-based image
		/// icons on Windows and all icons on Tizen currently render with their original colors regardless
		/// of this property.
		/// </remarks>
#if NETSTANDARD2_0
		Color? IconColor { get; }
#else
		Color? IconColor => null;
#endif

		/// <summary>
		/// Gets the color used for the item's label text.
		/// </summary>
		/// <remarks>
		/// When <see langword="null"/>, the label falls back to a color contrasting the item background.
		/// When set, that color is used as the label color.
		/// </remarks>
#if NETSTANDARD2_0
		new Color? TextColor { get; }
#else
		new Color? TextColor => null;
#endif
	}
}
