namespace Microsoft.Maui.Graphics
{
	public class PatternPaint : Paint
	{
		IPattern _pattern;

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
