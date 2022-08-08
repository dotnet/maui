using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class FillArcs : AbstractScenario
	{
		public readonly bool includeEllipses;

		public FillArcs(bool includeEllipses = false)
			: base(720, 1024)
		{
			this.includeEllipses = includeEllipses;
		}

		public override void Draw(ICanvas canvas)
		{
			if (includeEllipses)
			{
				canvas.FillColor = Colors.LightGrey;
				canvas.FillEllipse(50.5f, 10.5f, 150, 15);
				canvas.FillEllipse(250.5f, 10.5f, 150, 15);
				canvas.FillColor = Colors.Black;
			}

			canvas.FillColor = Colors.Black;
			canvas.FillArc(50.5f, 10.5f, 150, 15, 90, 270, false);
			canvas.FillArc(250.5f, 10.5f, 150, 15, 90, 270, false);

			canvas.SaveState();

			if (includeEllipses)
			{
				EllipseFillArcsOfDifferentSizesAndColors(canvas);
				EllipseFillShadowedRect(canvas);
			}

			FillArcsOfDifferentSizesAndColors(canvas);
			FillArcsWithAlpha(canvas);
			FillShadowedRect(canvas);

			canvas.RestoreState();

			if (includeEllipses)
			{
				canvas.FillColor = Colors.LightGrey;
				canvas.FillEllipse(50.5f, 30.5f, 150, 15);
				canvas.FillEllipse(250.5f, 30.5f, 150, 15);
				canvas.FillColor = Colors.Black;
			}

			canvas.FillColor = Colors.Black;
			canvas.FillArc(50.5f, 30.5f, 150, 15, 90, 270, true);
			canvas.FillArc(250.5f, 30.5f, 150, 15, 90, 270, true);
		}

		private static void EllipseFillShadowedRect(ICanvas canvas)
		{
			canvas.FillColor = Colors.LightGrey;
			canvas.FillEllipse(50.5f, 400.5f, 200, 50);
			canvas.FillColor = Colors.Black;
		}

		private static void FillShadowedRect(ICanvas canvas)
		{
			canvas.SaveState();
			canvas.FillColor = Colors.Black;
			canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
			canvas.FillArc(50.5f, 400.5f, 200, 50, 90, 270, true);

			canvas.RestoreState();
		}

		private static void EllipseFillArcsOfDifferentSizesAndColors(ICanvas canvas)
		{
			canvas.FillColor = Colors.LightGrey;
			for (int i = 1; i < 5; i++)
			{
				canvas.FillEllipse(50, 50 + i * 30, 150, 20);
				canvas.FillEllipse(250.5f, 50.5f + i * 30, 150, 20);
			}

			for (int i = 1; i < 5; i++)
			{
				canvas.FillEllipse(450.5f, 50.5f + i * 30, 150, 20);
			}

			canvas.FillColor = Colors.Black;
		}

		private static void FillArcsOfDifferentSizesAndColors(ICanvas canvas)
		{
			canvas.FillColor = Colors.Salmon;
			for (int i = 1; i < 5; i++)
			{
				canvas.FillArc(50, 50 + i * 30, 150, 20, 45, 180, false);
				canvas.FillArc(250.5f, 50.5f + i * 30, 150, 20, 45, 180, false);
			}

			canvas.FillColor = Colors.CornflowerBlue;
			for (int i = 1; i < 5; i++)
			{
				canvas.FillArc(450.5f, 50.5f + i * 30, 150, 20, 45, 180, false);
			}
		}

		private static void FillArcsWithAlpha(ICanvas canvas)
		{
			canvas.FillColor = Colors.Black;
			canvas.StrokeSize = 2;
			for (int i = 1; i <= 10; i++)
			{
				canvas.Alpha = (float)i / 10f;
				canvas.FillArc(450f, 200f + i * 30, 150, 20, 180, 0, true);
			}

			canvas.Alpha = 1;
		}

		public override string ToString()
		{
			if (includeEllipses)
			{
				return "FillArcs (Including Background Ellipses)";
			}

			return base.ToString();
		}
	}
}
