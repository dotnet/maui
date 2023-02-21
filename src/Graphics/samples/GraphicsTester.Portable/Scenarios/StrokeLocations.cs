using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class StrokeLocations : AbstractScenario
	{
		public StrokeLocations()
			: base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();

			canvas.StrokeColor = Colors.Black;
			canvas.StrokeSize = 10;
			canvas.Translate(0, -350);

			//
			// StrokeLocation.CENTER
			//
			canvas.StrokeSize = 10;
			canvas.DrawRectangle(50, 400, 100, 50);
			canvas.DrawEllipse(200, 400, 100, 50);
			canvas.DrawRoundedRectangle(350, 400, 100, 50, 25);

			var path = new PathF();
			path.MoveTo(550, 400);
			path.LineTo(500, 450);
			path.LineTo(600, 450);
			path.Close();
			canvas.DrawPath(path);

			canvas.RestoreState();
		}
	}
}
