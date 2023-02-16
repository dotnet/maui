using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class TransformRotateFromOrigin : AbstractScenario
	{
		public TransformRotateFromOrigin()
			: base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();
			canvas.StrokeColor = Colors.LightGrey;
			canvas.DrawRectangle(100, 100, 100, 100);
			canvas.StrokeColor = Colors.Black;
			canvas.Rotate(10);
			canvas.DrawRectangle(100, 100, 100, 100);
			canvas.StrokeColor = Colors.Salmon;
			canvas.Rotate(10);
			canvas.DrawRectangle(100, 100, 100, 100);
			canvas.StrokeColor = Colors.CornflowerBlue;
			canvas.Rotate(10);
			canvas.DrawRectangle(100, 100, 100, 100);
			canvas.RestoreState();

			canvas.StrokeColor = Colors.Blue;
			var point = new PointF(65, 65);
			for (int i = -3; i < 3; i++)
			{
				var rotated = GeometryUtil.RotatePoint(point, -15 * i);
				canvas.DrawLine(rotated.X - 10, rotated.Y, rotated.X + 10, rotated.Y);
				canvas.DrawLine(rotated.X, rotated.Y - 10, rotated.X, rotated.Y + 10);
			}

			canvas.SaveState();
			canvas.FillColor = Colors.Black;
			canvas.FillEllipse(60, 60, 10, 10);
			canvas.SetShadow(new SizeF(2, 0), 2, Colors.Black);
			canvas.StrokeColor = Colors.CornflowerBlue;
			canvas.Rotate(15);
			canvas.DrawEllipse(60, 60, 10, 10);
			canvas.Rotate(15);
			canvas.DrawEllipse(60, 60, 10, 10);
			canvas.Rotate(15);
			canvas.DrawEllipse(60, 60, 10, 10);
			canvas.StrokeColor = Colors.DarkSeaGreen;
			canvas.Rotate(-60);
			canvas.DrawEllipse(60, 60, 10, 10);
			canvas.Rotate(-15);
			canvas.DrawEllipse(60, 60, 10, 10);
			canvas.RestoreState();
		}
	}
}
