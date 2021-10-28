namespace Microsoft.Maui.Graphics
{
	public class BackgroundDrawable : IDrawable
	{
		Paint _paint;
		PathF _path;

		public BackgroundDrawable(Paint paint, PathF path)
		{
			_paint = paint;
			_path = path;
		}

		public void UpdatePaint(Paint paint)
		{
			_paint = paint;
		}

		public void UpdatePath(PathF path)
		{
			_path = path;
		}

		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			canvas.SaveState();

			canvas.SetFillPaint(_paint, dirtyRect);
			canvas.FillPath(_path);

			canvas.RestoreState();
		}
	}
}