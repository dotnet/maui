using System.IO;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a callback method that handles the layout of a line of text.
	/// </summary>
	/// <param name="aPoint">The position of the line.</param>
	/// <param name="aTextual">The text attributes to use for rendering the line.</param>
	/// <param name="aText">The text content of the line.</param>
	/// <param name="aAscent">The ascent of the line (distance from baseline to the top of the tallest glyph).</param>
	/// <param name="aDescent">The descent of the line (distance from baseline to the bottom of the deepest glyph).</param>
	/// <param name="aLeading">The leading of the line (extra space between lines).</param>
	public delegate void LayoutLine(PointF aPoint, ITextAttributes aTextual, string aText, float aAscent, float aDescent, float aLeading);
}
