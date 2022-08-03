using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class ArcScenario2 : AbstractScenario
	{
		public ArcScenario2()
			: base(720, 1024)
		{
		}

		private void DrawArc(ICanvas canvas, float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed)
		{
			canvas.FillColor = Colors.Black;
			canvas.FillArc(x, y, width, height, startAngle, endAngle, clockwise);

			var path = new PathF();
			path.AddArc(x, y + 400, x + width, y + 400 + width, startAngle, endAngle, clockwise);
			path.Close();
			canvas.FillPath(path);
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();
			DrawArc(canvas, 400, 100, 80, 80, -315, 300, false, false);
			canvas.RestoreState();
		}
	}
}
