namespace Microsoft.Maui.Graphics
{
	public static partial class PaintExtensions
	{
		public static IDrawable ToDrawable(this Paint paint)
		{
			return new BackgroundDrawable(paint);
		}

		class BackgroundDrawable : IDrawable
		{
			Paint _paint;
			public BackgroundDrawable(Paint paint)
			{
				// Workaround.
				// I think, SolidPaint violate LSP
				// When Paint was used on Canvas.SetFillPaint, BackgroundColor was referred
				// But SolidPaint do not update BackgroundColor when Color was updated even though it has same meaning.
				if (paint is SolidPaint solidPaint)
				{
					solidPaint.BackgroundColor = solidPaint.Color;
				}

				_paint = paint;
			}

			public void Draw(ICanvas canvas, RectangleF dirtyRect)
			{
				canvas.SaveState();

				canvas.SetFillPaint(_paint, dirtyRect);
				canvas.FillRectangle(dirtyRect);

				canvas.RestoreState();
			}
		}
	}
}
