using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class DrawArcs : AbstractScenario
	{
		public readonly bool includeEllipses;

		public DrawArcs(bool includeEllipses = false) : base(720, 1024)
		{
			this.includeEllipses = includeEllipses;
		}

		public override void Draw(ICanvas canvas)
		{
			if (includeEllipses)
			{
				canvas.StrokeColor = Colors.LightGrey;
				canvas.DrawEllipse(50.5f, 10.5f, 150, 15);
				canvas.DrawEllipse(250.5f, 10.5f, 150, 15);
				canvas.StrokeColor = Colors.Black;
			}

			canvas.DrawArc(50.5f, 10.5f, 150, 15, 90, 270, false, false);
			canvas.DrawArc(250.5f, 10.5f, 150, 15, 90, 270, false, true);

			canvas.SaveState();

			if (includeEllipses)
			{
				EllipseDrawArcsOfDifferentSizesAndColors(canvas);
				EllipseDrawArcsWithDashesOfDifferentSizes(canvas);
				EllipseDrawShadowedRect(canvas);
				EllipseDrawArcsWithDifferentStrokeLocations(canvas);
			}

			DrawArcsOfDifferentSizesAndColors(canvas);
			DrawArcsWithDashesOfDifferentSizes(canvas);
			DrawArcsWithAlpha(canvas);
			DrawShadowedRect(canvas);
			DrawArcsWithDifferentStrokeLocations(canvas);

			canvas.RestoreState();

			if (includeEllipses)
			{
				canvas.StrokeColor = Colors.LightGrey;
				canvas.DrawEllipse(50.5f, 30.5f, 150, 15);
				canvas.DrawEllipse(250.5f, 30.5f, 150, 15);
				canvas.StrokeColor = Colors.Black;
			}

			canvas.DrawArc(50.5f, 30.5f, 150, 15, 90, 270, true, false);
			canvas.DrawArc(250.5f, 30.5f, 150, 15, 90, 270, true, true);
		}

		private static void EllipseDrawShadowedRect(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.LightGrey;
			canvas.StrokeSize = 5;
			canvas.DrawEllipse(50.5f, 400.5f, 200, 50);
			canvas.StrokeColor = Colors.Black;
		}

		private static void DrawShadowedRect(ICanvas canvas)
		{
			canvas.SaveState();
			canvas.StrokeColor = Colors.Black;
			canvas.StrokeSize = 5;
			canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
			canvas.DrawArc(50.5f, 400.5f, 200, 50, 90, 270, true, false);

			canvas.RestoreState();
		}

		private static void EllipseDrawArcsWithDashesOfDifferentSizes(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.LightGrey;
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.DrawEllipse(50f, 200f + i * 30, 150, 20);
				canvas.DrawEllipse(250.5f, 200.5f + i * 30, 150, 20);
			}

			canvas.StrokeColor = Colors.Black;
		}

		private static void DrawArcsWithDashesOfDifferentSizes(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.Salmon;
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.StrokeDashPattern = DASHED;
				canvas.StrokeDashOffset = 2;
				canvas.DrawArc(50f, 200f + i * 30, 150, 20, 0, 180, false, false);
				canvas.DrawArc(250.5f, 200.5f + i * 30, 150, 20, 0, 180, false, false);
			}

			canvas.StrokeDashPattern = SOLID;
		}

		private static void EllipseDrawArcsOfDifferentSizesAndColors(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.LightGrey;
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.DrawEllipse(50, 50 + i * 30, 150, 20);
				canvas.DrawEllipse(250.5f, 50.5f + i * 30, 150, 20);
			}

			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.DrawEllipse(450.5f, 50.5f + i * 30, 150, 20);
			}

			canvas.StrokeColor = Colors.Black;
		}

		private static void DrawArcsOfDifferentSizesAndColors(ICanvas canvas)
		{
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.DrawArc(50, 50 + i * 30, 150, 20, 45, 180, false, false);
				canvas.DrawArc(250.5f, 50.5f + i * 30, 150, 20, 45, 180, false, false);
			}

			canvas.StrokeColor = Colors.CornflowerBlue;
			for (int i = 1; i < 5; i++)
			{
				canvas.StrokeSize = i;
				canvas.DrawArc(450.5f, 50.5f + i * 30, 150, 20, 45, 180, false, false);
			}
		}

		private static void DrawArcsWithAlpha(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.Black;
			canvas.StrokeSize = 2;
			for (int i = 1; i <= 10; i++)
			{
				canvas.Alpha = (float)i / 10f;
				canvas.DrawArc(450f, 200f + i * 30, 150, 20, 180, 0, true, true);
			}

			canvas.Alpha = 1;
		}

		private static void EllipseDrawArcsWithDifferentStrokeLocations(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.LightGrey;
			for (int i = 1; i < 4; i++)
			{
				canvas.StrokeSize = i * 2 + 1;
				canvas.DrawEllipse(50.5f, 500.5f + i * 40, 150, 20);
			}

			canvas.StrokeColor = Colors.Black;
		}

		private static void DrawArcsWithDifferentStrokeLocations(ICanvas canvas)
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
				canvas.DrawArc(50.5f, 500.5f + i * 40, 150, 20, 0, 180, false, false);
			}
		}

		public override string ToString()
		{
			if (includeEllipses)
			{
				return "DrawArcs (Including Background Ellipses)";
			}

			return base.ToString();
		}
	}
}
