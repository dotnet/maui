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
