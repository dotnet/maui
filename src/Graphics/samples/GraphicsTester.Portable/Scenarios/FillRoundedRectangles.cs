using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class FillRoundedRectangles : AbstractScenario
	{
		public FillRoundedRectangles()
			: base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			FillRoundedRectanglesOfDifferentSizesAndColors(canvas);
			FillRoundedRectanglesWithAlpha(canvas);
			FillShadowedRect(canvas);
			FillRoundedRectWithZeroAndLargeRadius(canvas);
			FillRoundedWithDifferentXYRadius(canvas);
		}

		private static void FillShadowedRect(ICanvas canvas)
		{
			canvas.SaveState();
			canvas.FillColor = Colors.Black;
			canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
			canvas.FillRoundedRectangle(50.5f, 400.5f, 200, 50, 10);
			canvas.RestoreState();
		}

		private static void FillRoundedRectanglesOfDifferentSizesAndColors(ICanvas canvas)
		{
			canvas.FillColor = Colors.Salmon;
			for (int i = 1; i < 5; i++)
			{
				canvas.FillRoundedRectangle(50, 50 + i * 30, 150, 20, 5);
			}

			canvas.FillColor = Colors.CornflowerBlue;
			for (int i = 1; i < 5; i++)
			{
				canvas.FillRoundedRectangle(250.5f, 50.5f + i * 30, 150, 20, 5);
			}
		}

		private static void FillRoundedRectanglesWithAlpha(ICanvas canvas)
		{
			canvas.FillColor = Colors.Black;
			for (int i = 1; i <= 10; i++)
			{
				canvas.Alpha = (float)i / 10f;
				canvas.FillRoundedRectangle(450f, 200f + i * 30, 150, 20, 5);
			}

			canvas.Alpha = 1;
		}

		private void FillRoundedRectWithZeroAndLargeRadius(ICanvas canvas)
		{
			canvas.FillColor = Colors.Blue;
			canvas.FillRoundedRectangle(250.5f, 700.5f, 150, 20, 0);
			canvas.FillRoundedRectangle(450.5f, 700.5f, 150, 20, 50);
		}

		private void FillRoundedWithDifferentXYRadius(ICanvas canvas)
		{
			RectF rect = new RectF(275, 400, 100, 100);
			canvas.FillRoundedRectangle(rect, xRadius: 20, yRadius: 40);
		}
	}
}
