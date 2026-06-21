namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines an interface for a picture, which is a collection of drawing commands that can be replayed on a canvas.
	/// </summary>
	public interface IPicture
	{
		/// <summary>
		/// Draws this picture onto the specified canvas.
		/// </summary>
		/// <param name="canvas">The canvas to draw onto.</param>
		void Draw(ICanvas canvas);

		/// <summary>
		/// Gets the x-coordinate of the picture's origin.
		/// </summary>
		float X { get; }

		/// <summary>
		/// Gets the y-coordinate of the picture's origin.
		/// </summary>
		float Y { get; }

		/// <summary>
		/// Gets the width of the picture.
		/// </summary>
		float Width { get; }

		/// <summary>
		/// Gets the height of the picture.
		/// </summary>
		float Height { get; }
	}
}
