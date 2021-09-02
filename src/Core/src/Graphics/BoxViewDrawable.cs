using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Graphics
{
	public class BoxViewDrawable : IDrawable
	{
		public BoxViewDrawable()
		{

		}

		public BoxViewDrawable(IBoxView? boxView)
		{
			BoxView = boxView;
		}

		public IBoxView? BoxView { get; set; }

		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			var rect = dirtyRect;

			var path = new PathF();

			float x = dirtyRect.X;
			float y = dirtyRect.Y;
			float w = dirtyRect.Width;
			float h = dirtyRect.Height;

			float topLeftCornerRadius = 0;
			float topRightCornerRadius = 0;
			float bottomLeftCornerRadius = 0;
			float bottomRightCornerRadius = 0;

			if (BoxView?.CornerRadius != null)
			{
				topLeftCornerRadius = (float)BoxView.CornerRadius.TopLeft;
				topRightCornerRadius = (float)BoxView.CornerRadius.TopRight;
				bottomLeftCornerRadius = (float)BoxView.CornerRadius.BottomLeft;
				bottomRightCornerRadius = (float)BoxView.CornerRadius.BottomRight;
			}

			path.AppendRoundedRectangle(x, y, w, h, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius);

			DrawBoxView(canvas, rect, path);
		}

		void DrawBoxView(ICanvas canvas, RectangleF dirtyRect, PathF path)
		{
			if (BoxView == null)
				return;

			if (!path.Closed)
				return;

			canvas.SaveState();

			// Set BackgroundColor
			if (BoxView.Color != null)
				canvas.FillColor = BoxView.Color;
			else
				canvas.SetFillPaint(BoxView.Background, dirtyRect);

			canvas.FillPath(path);

			canvas.RestoreState();
		}
	}
}
