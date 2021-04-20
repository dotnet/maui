using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class DrawLinesScaled : AbstractScenario
	{
		public DrawLinesScaled() : base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.StrokeSize = 1;
			canvas.StrokeDashPattern = DOT_DOT;
			canvas.DrawLine(50, 20f, 200, 20f);

			canvas.StrokeSize = 1;
			canvas.StrokeDashPattern = SOLID;
			canvas.DrawLine(50, 30f, 200, 30f);

			canvas.SaveState();

			canvas.Scale(2, 2);

			canvas.StrokeSize = 1;
			canvas.StrokeDashPattern = DOT_DOT;
			canvas.DrawLine(50, 20f, 200, 20f);

			canvas.StrokeSize = 1;
			canvas.StrokeDashPattern = SOLID;
			canvas.DrawLine(50, 30f, 200, 30f);

			canvas.RestoreState();
		}
	}
}
