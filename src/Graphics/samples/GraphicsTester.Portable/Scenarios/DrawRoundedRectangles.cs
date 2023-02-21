using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class DrawRoundedRectangles : AbstractScenario
	{
		public DrawRoundedRectangles() : base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.DrawRoundedRectangle(50.5f, 10.5f, 150, 15, 5);

			canvas.SaveState();

			DrawRoundedRectanglesOfDifferentSizesAndColors(canvas);
			DrawRoundedRectanglesWithDashesOfDifferentSizes(canvas);
			DrawRoundedRectanglesWithAlpha(canvas);
			DrawShadowedRect(canvas);
			DrawRoundedRectanglesWithDifferentStrokeLocations(canvas);
			DrawRoundedRectWithZeroAndLargeRadius(canvas);
			DrawRoundedWithDifferentXYRadius(canvas);
			DrawRoundedWithCircles(canvas);
			canvas.RestoreState();

			canvas.DrawRoundedRectangle(50.5f, 30.5f, 150, 15, 5);
		}

		private static void DrawShadowedRect(ICanvas canvas)
		{
			canvas.SaveState();
			canvas.StrokeColor = Colors.Black;
			canvas.StrokeSize = 5;
			canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
			canvas.DrawRoundedRectangle(50.5f, 400.5f, 200, 50, 10);

			canvas.RestoreState();
		}

		private static void DrawRoundedRectanglesWithDashesOfDifferentSizes(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.Salmon;
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.StrokeDashPattern = DASHED;
				canvas.StrokeDashOffset = 2;
				canvas.DrawRoundedRectangle(50f, 200f + i * 30, 150, 20, 5);
				canvas.DrawRoundedRectangle(250.5f, 200.5f + i * 30, 150, 20, 5);
			}

			canvas.StrokeDashPattern = SOLID;
		}

		private static void DrawRoundedRectanglesOfDifferentSizesAndColors(ICanvas canvas)
		{
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.DrawRoundedRectangle(50, 50 + i * 30, 150, 20, 5);
				canvas.DrawRoundedRectangle(250.5f, 50.5f + i * 30, 150, 20, 5);
			}

			canvas.StrokeColor = Colors.CornflowerBlue;
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.DrawRoundedRectangle(450.5f, 50.5f + i * 30, 150, 20, 5);
			}
		}

		private static void DrawRoundedRectanglesWithAlpha(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.Black;
			canvas.StrokeSize = 2;
			for (int i = 1; i <= 10; i++)
			{
				canvas.Alpha = (float)i / 10f;
				canvas.DrawRoundedRectangle(450f, 200f + i * 30, 150, 20, 5);
			}

			canvas.Alpha = 1;
		}

		private static void DrawRoundedRectanglesWithDifferentStrokeLocations(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.Blue;
			canvas.StrokeSize = 1;
			canvas.DrawLine(0, 540.5f, 650, 540.5f);
			canvas.DrawLine(0, 580.5f, 650, 580.5f);
			canvas.DrawLine(0, 620.5f, 650, 620.5f);

			canvas.StrokeColor = Colors.ForestGreen;
			for (int i = 1; i < 4; i++)
			{
				canvas.StrokeSize = i * 2 + 1;
				canvas.DrawRoundedRectangle(50.5f, 500.5f + i * 40, 150, 20, 5);
			}
		}

		private void DrawRoundedRectWithZeroAndLargeRadius(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.Blue;
			canvas.StrokeSize = 1;
			canvas.DrawRoundedRectangle(250.5f, 700.5f, 150, 20, 0);
			canvas.DrawRoundedRectangle(450.5f, 700.5f, 150, 20, 50);
		}

		private void DrawRoundedWithDifferentXYRadius(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.Blue;
			canvas.StrokeSize = 1;
			RectF rect = new RectF(275, 400, 100, 100);
			canvas.DrawRoundedRectangle(rect, xRadius: 20, yRadius: 40);
		}

		private void DrawRoundedWithCircles(ICanvas canvas)
		{
			float circleRadius = 64;

			canvas.StrokeSize = .5f;
			canvas.StrokeColor = Colors.Magenta;
			RectF rect = new RectF(50, 740, circleRadius * 4, circleRadius * 4);
			canvas.DrawRoundedRectangle(rect, xRadius: circleRadius, yRadius: circleRadius);

			PointF[] circleCenters =
			{
				new PointF(rect.Left + circleRadius, rect.Top + circleRadius),
				new PointF(rect.Right - circleRadius, rect.Top + circleRadius),
				new PointF(rect.Left + circleRadius, rect.Bottom - circleRadius),
				new PointF(rect.Right - circleRadius, rect.Bottom - circleRadius),
			};

			canvas.StrokeColor = Colors.Green;
			foreach (PointF circleCenter in circleCenters)
			{
				canvas.DrawCircle(circleCenter, circleRadius);
				canvas.DrawCircle(circleCenter, 1);
			}
		}
	}
}
