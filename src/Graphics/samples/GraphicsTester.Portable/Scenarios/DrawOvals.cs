using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class DrawEllipses : AbstractScenario
	{
		public DrawEllipses() : base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.DrawEllipse(50.5f, 20.5f, 150, 5);

			canvas.SaveState();

			DrawEllipsesOfDifferentSizesAndColors(canvas);
			DrawEllipsesWithDashesOfDifferentSizes(canvas);
			DrawEllipsesWithAlpha(canvas);
			DrawShadowedRect(canvas);
			DrawEllipsesWithDifferentStrokeLocations(canvas);

			canvas.RestoreState();

			canvas.DrawEllipse(50.5f, 30.5f, 150, 5);
		}

		private static void DrawShadowedRect(ICanvas canvas)
		{
			canvas.SaveState();
			canvas.StrokeColor = Colors.Black;
			canvas.StrokeSize = 5;
			canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
			canvas.DrawEllipse(50.5f, 400.5f, 200, 50);

			canvas.RestoreState();
		}

		private static void DrawEllipsesWithDashesOfDifferentSizes(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.Salmon;
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.StrokeDashPattern = DASHED;
				canvas.StrokeDashOffset = 2;
				canvas.DrawEllipse(50f, 200f + i * 30, 150, 20);
				canvas.DrawEllipse(250.5f, 200.5f + i * 30, 150, 20);
			}

			canvas.StrokeDashPattern = SOLID;
		}

		private static void DrawEllipsesOfDifferentSizesAndColors(ICanvas canvas)
		{
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.DrawEllipse(50, 50 + i * 30, 150, 20);
				canvas.DrawEllipse(250.5f, 50.5f + i * 30, 150, 20);
			}

			canvas.StrokeColor = Colors.CornflowerBlue;
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.DrawEllipse(450.5f, 50.5f + i * 30, 150, 20);
			}
		}

		private static void DrawEllipsesWithAlpha(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.Black;
			canvas.StrokeSize = 2;
			for (int i = 1; i <= 10; i++)
			{
				canvas.Alpha = (float)i / 10f;
				canvas.DrawEllipse(450f, 200f + i * 30, 150, 20);
			}

			canvas.Alpha = 1;
		}

		private static void DrawEllipsesWithDifferentStrokeLocations(ICanvas canvas)
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
				canvas.DrawEllipse(50.5f, 500.5f + i * 40, 150, 20);
			}
		}
	}
}
