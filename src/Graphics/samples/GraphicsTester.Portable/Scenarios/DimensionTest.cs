using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class DimensionTest : AbstractScenario
	{
		public DimensionTest() : base(400, 300)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			// like array Length, image Width and Height are 1 unit larger than zero-indexed positions.
			PointF topLeft = new PointF(0, 0);
			PointF topRight = new PointF(Width - 1, 0);
			PointF bottomLeft = new PointF(0, Height - 1);
			PointF bottomRight = new PointF(Width - 1, Height - 1);

			canvas.FillColor = Colors.LightSlateGray;
			canvas.FillRectangle(0, 0, Width, Height);

			// draw lines instead of a rectangle becuase it's more obvious which pixel row/column is filled
			canvas.StrokeColor = Colors.Black;
			canvas.DrawLine(topLeft, topRight);
			canvas.DrawLine(topRight, bottomRight);
			canvas.DrawLine(bottomRight, bottomLeft);
			canvas.DrawLine(bottomLeft, topLeft);
			canvas.DrawLine(topLeft, bottomRight);
			canvas.DrawLine(topRight, bottomLeft);

			canvas.FontColor = Colors.Black;
			canvas.FontSize = 16f;
			canvas.DrawString($"{Width}x{Height}", Width / 2, 50, HorizontalAlignment.Center);
		}
	}
}
