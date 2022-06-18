using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class RadialGradientInCircle : AbstractScenario
	{
		public RadialGradientInCircle() : base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();

			var radialGradientPaint = new RadialGradientPaint
			{
				StartColor = Colors.White,
				EndColor = Colors.Black
			};

			radialGradientPaint.Center = new Point(0.5, 0.5);
			radialGradientPaint.Radius = 0.5;

			var ellipseRect1 = new RectF(100, 100, 200, 200);
			canvas.SetFillPaint(radialGradientPaint, ellipseRect1);
			canvas.FillEllipse(ellipseRect1);

			radialGradientPaint.Center = new Point(0.6, 0.7);
			radialGradientPaint.Radius = 0.5;

			var ellipseRect2 = new RectF(100, 400, 200, 200);
			canvas.SetFillPaint(radialGradientPaint, ellipseRect2);
			canvas.FillEllipse(ellipseRect2);

			canvas.RestoreState();
		}
	}
}
