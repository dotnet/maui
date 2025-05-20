namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a pattern that applies a paint to a wrapped pattern for filling shapes.
	/// </summary>
	public class PaintPattern : IPattern
	{
		/// <summary>
		/// Gets the wrapped pattern that this pattern applies a paint to.
		/// </summary>
		public IPattern Wrapped { get; }

		/// <summary>
		/// Gets or sets the paint to apply to the wrapped pattern.
		/// </summary>
		public Paint Paint { get; set; }

		/// <summary>
		/// Gets the width of the pattern, which is the width of the wrapped pattern or 0 if there is no wrapped pattern.
		/// </summary>
		public float Width => Wrapped?.Width ?? 0;

		/// <summary>
		/// Gets the height of the pattern, which is the height of the wrapped pattern or 0 if there is no wrapped pattern.
		/// </summary>
		public float Height => Wrapped?.Height ?? 0;

		/// <summary>
		/// Gets the horizontal step size for repeating the pattern, which is the step X of the wrapped pattern or 0 if there is no wrapped pattern.
		/// </summary>
		public float StepX => Wrapped?.StepX ?? 0;

		/// <summary>
		/// Gets the vertical step size for repeating the pattern, which is the step Y of the wrapped pattern or 0 if there is no wrapped pattern.
		/// </summary>
		public float StepY => Wrapped?.StepY ?? 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="PaintPattern"/> class with the specified wrapped pattern.
		/// </summary>
		/// <param name="pattern">The pattern to wrap and apply a paint to.</param>
		public PaintPattern(IPattern pattern)
		{
			Wrapped = pattern;
		}

		/// <summary>
		/// Draws the pattern onto the specified canvas with the current paint.
		/// </summary>
		/// <param name="canvas">The canvas to draw the pattern onto.</param>
		public void Draw(ICanvas canvas)
		{
			if (Paint != null)
			{
				if (Paint.BackgroundColor != null && Paint.BackgroundColor.Alpha > 1)
				{
					canvas.FillColor = Paint.BackgroundColor;
					canvas.FillRectangle(0, 0, Width, Height);
				}

				canvas.StrokeColor = Paint.ForegroundColor;
				canvas.FillColor = Paint.ForegroundColor;
			}
			else
			{
				canvas.StrokeColor = Colors.Black;
				canvas.FillColor = Colors.Black;
			}

			Wrapped.Draw(canvas);
		}
	}
}
