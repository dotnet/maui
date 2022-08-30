using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class TestPattern2 : AbstractScenario
	{
		public TestPattern2() : base(3500, 1700)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			var blurrableCanvas = canvas as IBlurrableCanvas;

			canvas.SaveState();
			DrawStrokes(canvas);
			canvas.RestoreState();

			canvas.SaveState();
			canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
			canvas.Translate(0, 100);
			DrawStrokes(canvas);
			canvas.RestoreState();

			canvas.SaveState();
			if (blurrableCanvas != null)
				blurrableCanvas.SetBlur(5);
			canvas.Translate(0, 200);
			DrawStrokes(canvas);
			canvas.RestoreState();

			//
			// FillXXXX Methods
			//

			canvas.SaveState();
			canvas.Translate(0, 300);
			DrawFills(canvas);
			canvas.RestoreState();

			canvas.SaveState();
			canvas.SetShadow(CanvasDefaults.DefaultShadowOffset, CanvasDefaults.DefaultShadowBlur, CanvasDefaults.DefaultShadowColor);
			canvas.Translate(0, 400);
			DrawFills(canvas);
			canvas.RestoreState();

			canvas.SaveState();
			if (blurrableCanvas != null)
				blurrableCanvas.SetBlur(5);
			canvas.Translate(0, 500);
			DrawFills(canvas);
			canvas.RestoreState();
		}

		private static void DrawFills(ICanvas canvas)
		{
			canvas.FillColor = Colors.Red;
			canvas.FillRectangle(10, 10, 80, 80);

			canvas.FillColor = Colors.Green;
			canvas.Translate(100, 0);
			canvas.FillEllipse(10, 10, 80, 80);

			canvas.FillColor = Colors.Blue;
			canvas.Translate(100, 0);
			canvas.FillRoundedRectangle(10, 10, 80, 80, 10);

			canvas.FillColor = Colors.CornflowerBlue;
			var path = new PathF();
			path.MoveTo(10, 10);
			path.LineTo(50, 90);
			path.LineTo(90, 10);
			path.Close();
			canvas.Translate(100, 0);
			canvas.FillPath(path);
		}

		private static void DrawStrokes(ICanvas canvas)
		{
			//
			// DrawXXXX Methods
			//
			canvas.DrawLine(0, 0, 100, 100);
			canvas.Translate(100, 0);
			canvas.DrawRectangle(0, 0, 100, 100);
			canvas.Translate(100, 0);
			canvas.DrawEllipse(0, 0, 100, 100);
			canvas.Translate(100, 0);
			canvas.DrawRoundedRectangle(0, 0, 100, 100, 25);

			var vPath = new PathF();
			vPath.MoveTo(0, 0);
			vPath.LineTo(0, 100);
			vPath.QuadTo(100, 100, 100, 0);
			vPath.CurveTo(50, 0, 100, 50, 50, 50);
			canvas.Translate(100, 0);
			canvas.DrawPath(vPath);

			canvas.Translate(100, 0);
			canvas.DrawRectangle(0, 0, 100, 50);
			canvas.Translate(100, 0);
			canvas.DrawEllipse(0, 0, 100, 50);
			canvas.Translate(100, 0);
			canvas.DrawRoundedRectangle(0, 0, 100, 50, 25);
			canvas.Translate(100, 0);
			canvas.DrawRoundedRectangle(0, 0, 100, 25, 25);
		}
	}
}
