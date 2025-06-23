namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines an object that can be drawn onto a canvas.
	/// </summary>
	public interface IDrawable
	{
		/// <summary>
		/// Draws the content onto the specified canvas within the given rectangle.
		/// </summary>
		/// <param name="canvas">The canvas to draw onto.</param>
		/// <param name="dirtyRect">The rectangle in which to draw.</param>
		void Draw(ICanvas canvas, RectF dirtyRect);
	}
}
