using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class FilledArcDirection : AbstractScenario
	{
		public FilledArcDirection()
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

			DrawArc(canvas, 100, 100, 80, 80, 45, 300, true, false);
			DrawArc(canvas, 200, 100, 80, 80, 45, 300, false, false);
			DrawArc(canvas, 300, 100, 80, 80, -315, 300, true, false);
			DrawArc(canvas, 400, 100, 80, 80, -315, 300, false, false);

			DrawArc(canvas, 100, 200, 80, 80, 45, -60, true, false);
			DrawArc(canvas, 200, 200, 80, 80, 45, -60, false, false);
			DrawArc(canvas, 300, 200, 80, 80, -315, -60, true, false);
			DrawArc(canvas, 400, 200, 80, 80, -315, -60, false, false);

			DrawArc(canvas, 100, 300, 80, 80, 270, 45, true, false);
			DrawArc(canvas, 200, 300, 80, 80, 270, 45, false, false);
			DrawArc(canvas, 300, 300, 80, 80, -90, 45, true, false);
			DrawArc(canvas, 400, 300, 80, 80, -90, 45, false, false);

			canvas.RestoreState();
		}
	}
}
