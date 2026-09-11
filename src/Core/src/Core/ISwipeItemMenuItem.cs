using System.ComponentModel;
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

	/// <summary>
	/// Provides an optional icon tint for an <see cref="ISwipeItemMenuItem"/>.
	/// </summary>
	/// <remarks>
	/// This is a separate optional interface to preserve compatibility for existing
	/// <see cref="ISwipeItemMenuItem"/> implementations, including netstandard2.0 targets
	/// where adding the member to the existing interface would require implementers to add it.
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ISwipeItemMenuItemIconColor
	{
		/// <summary>
		/// Gets the color used to tint the icon specified by <see cref="IImageSourcePart.Source"/>.
		/// </summary>
		/// <remarks>
		/// When <see langword="null"/>, a font icon is tinted by its own color, then by the item's
		/// <see cref="ITextStyle.TextColor"/>, then by a color contrasting the item background;
		/// image icons render with their original colors. When set, the color tints font icons and image
		/// icons on Android, iOS, and MacCatalyst (including stream-based sources, since the resolved
		/// platform image is tinted), and font and packaged file icons on Windows. On Windows,
		/// packaged file icons use the color as a monochrome mask; URI, rooted, and stream-based image
		/// icons render with their original colors. All icons on Tizen currently render with their
		/// original colors regardless of this property.
		/// </remarks>
		Color? IconColor { get; }
	}
}
