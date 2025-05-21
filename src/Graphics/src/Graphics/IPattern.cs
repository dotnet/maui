namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines a pattern that can be used to fill shapes on a canvas.
	/// </summary>
	public interface IPattern
	{
		/// <summary>
		/// Gets the width of the pattern.
		/// </summary>
		float Width { get; }

		/// <summary>
		/// Gets the height of the pattern.
		/// </summary>
		float Height { get; }

		/// <summary>
		/// Gets the horizontal step size for repeating the pattern.
		/// </summary>
		float StepX { get; }

		/// <summary>
		/// Gets the vertical step size for repeating the pattern.
		/// </summary>
		float StepY { get; }

		/// <summary>
		/// Draws the pattern onto the specified canvas.
		/// </summary>
		/// <param name="canvas">The canvas to draw the pattern on.</param>
		void Draw(ICanvas canvas);
	}
}
