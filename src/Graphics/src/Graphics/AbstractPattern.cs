namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides a base implementation for pattern types that can be used to fill shapes.
	/// </summary>
	public abstract class AbstractPattern : IPattern
	{
		/// <summary>
		/// Gets the width of the pattern.
		/// </summary>
		public float Width { get; }

		/// <summary>
		/// Gets the height of the pattern.
		/// </summary>
		public float Height { get; }

		/// <summary>
		/// Gets the horizontal step size for repeating the pattern.
		/// </summary>
		public float StepX { get; }

		/// <summary>
		/// Gets the vertical step size for repeating the pattern.
		/// </summary>
		public float StepY { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AbstractPattern"/> class with the specified dimensions and step sizes.
		/// </summary>
		/// <param name="width">The width of the pattern.</param>
		/// <param name="height">The height of the pattern.</param>
		/// <param name="stepX">The horizontal step size for repeating the pattern.</param>
		/// <param name="stepY">The vertical step size for repeating the pattern.</param>
		protected AbstractPattern(float width, float height, float stepX, float stepY)
		{
			Width = width;
			Height = height;
			StepX = stepX;
			StepY = stepY;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AbstractPattern"/> class with the specified dimensions.
		/// </summary>
		/// <param name="width">The width of the pattern.</param>
		/// <param name="height">The height of the pattern.</param>
		/// <remarks>Sets both step sizes equal to the pattern dimensions, creating a non-overlapping tiled pattern.</remarks>
		protected AbstractPattern(float width, float height) : this(width, height, width, height)
		{
		}

		protected AbstractPattern(float stepSize) : this(stepSize, stepSize)
		{
		}

		public abstract void Draw(ICanvas canvas);
	}
}
