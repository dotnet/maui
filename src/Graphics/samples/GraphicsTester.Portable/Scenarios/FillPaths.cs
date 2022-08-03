using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class FillPaths : AbstractScenario
	{
		public FillPaths() : base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			FillQuadraticSegment(canvas);
			FillCubicSegment(canvas);
			FillArcSegment(canvas);
			FillPie(canvas);
			FillCubicAndQuad(canvas);
			FillSubPaths(canvas);
		}

		private void FillQuadraticSegment(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.LightGrey;
			canvas.DrawRectangle(150.5f, 50.5f, 50, 50);
			var path = new PathF(150.5f, 50.5f);
			path.QuadTo(150.5f, 100.5f, 200.5f, 100.5f);
			canvas.FillColor = Colors.Black;
			canvas.FillPath(path);
		}

		private void FillCubicSegment(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.LightGrey;
			canvas.DrawRectangle(250.5f, 50.5f, 50, 50);
			var path = new PathF(250.5f, 50.5f);
			path.CurveTo(250.5f, 100.5f, 300.5f, 50.5f, 300.5f, 100.5f);
			canvas.FillColor = Colors.Black;
			canvas.FillPath(path);
		}

		private void FillArcSegment(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.LightGrey;
			canvas.DrawRectangle(350.5f, 50.5f, 50, 50);
			var path = new PathF();
			path.AddArc(350.5f, 50.5f, 400.5f, 100.5f, 45f, 135, false);
			canvas.FillColor = Colors.Black;
			canvas.FillPath(path);
		}

		private void FillPie(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.LightGrey;
			canvas.DrawRectangle(350.5f, 150.5f, 50, 50);
			var path = new PathF();
			path.AddArc(350.5f, 150.5f, 400.5f, 200.5f, 45f, 135, false);
			path.LineTo(375.5f, 200.5f);
			path.Close();
			canvas.FillColor = Colors.Black;
			canvas.FillPath(path);
		}

		private void FillCubicAndQuad(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.LightGrey;
			canvas.DrawRectangle(250.5f, 150.5f, 50, 50);
			var path = new PathF(250.5f, 150.5f);
			path.CurveTo(250.5f, 200.5f, 300.5f, 150.5f, 300.5f, 200.5f);
			path.QuadTo(300.5f, 150.5f, 250.5f, 150.5f);
			path.Close();
			canvas.FillColor = Colors.Black;
			canvas.FillPath(path);
		}

		private void FillSubPaths(ICanvas canvas)
		{
			canvas.StrokeColor = Colors.LightGrey;
			canvas.DrawRectangle(150.5f, 150.5f, 50, 50);
			var path = new PathF();
			path.AppendRectangle(175.5f, 150.5f, 25, 50);
			path.AppendEllipse(175.5f, 150.5f, 25, 50);
			canvas.FillColor = Colors.Black;
			canvas.FillPath(path);
		}
	}
}
