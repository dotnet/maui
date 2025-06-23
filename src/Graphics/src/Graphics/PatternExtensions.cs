namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for the <see cref="IPattern"/> interface.
	/// </summary>
	public static class PatternExtensions
	{
		/// <summary>
		/// Converts a pattern to a paint using black as the foreground color.
		/// </summary>
		/// <param name="target">The pattern to convert.</param>
		/// <returns>A <see cref="Paint"/> configured with the pattern and black foreground color, or null if the pattern is null.</returns>
		public static Paint AsPaint(this IPattern target)
		{
			return AsPaint(target, Colors.Black);
		}

		/// <summary>
		/// Converts a pattern to a paint using the specified foreground color.
		/// </summary>
		/// <param name="target">The pattern to convert.</param>
		/// <param name="foregroundColor">The foreground color to use for the pattern.</param>
		/// <returns>A <see cref="Paint"/> configured with the pattern and specified foreground color, or null if the pattern is null.</returns>
		public static Paint AsPaint(this IPattern target, Color foregroundColor)
		{
			if (target != null)
			{
				var paint = new PatternPaint
				{
					Pattern = target,
					ForegroundColor = foregroundColor,
					BackgroundColor = null
				};
				return paint;
			}

			return null;
		}
	}
}
