using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class LineJoins : AbstractScenario
	{
		public LineJoins() : base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();

			canvas.StrokeColor = Colors.Black;
			canvas.StrokeSize = 10;
			canvas.Translate(-300, -60);

			var path = new PathF();
			path.MoveTo(350, 120);
			path.LineTo(370, 180);
			path.LineTo(390, 120);
			canvas.DrawPath(path);

			canvas.StrokeLineJoin = LineJoin.Miter;
			path = new PathF();
			path.MoveTo(400, 120);
			path.LineTo(420, 180);
			path.LineTo(440, 120);
			canvas.DrawPath(path);

			canvas.StrokeLineJoin = LineJoin.Round;
			path = new PathF();
			path.MoveTo(450, 120);
			path.LineTo(470, 180);
			path.LineTo(490, 120);
			canvas.DrawPath(path);

			canvas.StrokeLineJoin = LineJoin.Bevel;
			path = new PathF();
			path.MoveTo(500, 120);
			path.LineTo(520, 180);
			path.LineTo(540, 120);
			canvas.DrawPath(path);

			canvas.StrokeLineJoin = LineJoin.Miter;
			canvas.MiterLimit = 2;
			path = new PathF();
			path.MoveTo(550, 120);
			path.LineTo(570, 180);
			path.LineTo(590, 120);
			canvas.DrawPath(path);

			canvas.RestoreState();
		}
	}
}
