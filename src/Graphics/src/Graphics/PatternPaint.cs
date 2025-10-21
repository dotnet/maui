namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a paint that fills shapes with a repeating pattern.
	/// </summary>
	public class PatternPaint : Paint
	{
		IPattern _pattern;

		/// <summary>
		/// Gets or sets the pattern used for filling shapes.
		/// </summary>
		/// <remarks>
		/// If the pattern is not already a <see cref="PaintPattern"/>, it will be wrapped in one automatically.
		/// </remarks>
		public IPattern Pattern
		{
			get => _pattern;

			set
			{
				_pattern = value;

				if (!(_pattern is PaintPattern))
				{
					_pattern = new PaintPattern(_pattern) { Paint = this };
				}
			}
		}

		public override bool IsTransparent
		{
			get
			{
				if (BackgroundColor == null || BackgroundColor.Alpha < 1)
					return true;

				return ForegroundColor.Alpha < 1;
			}
		}
	}
}
