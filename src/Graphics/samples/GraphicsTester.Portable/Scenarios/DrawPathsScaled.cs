using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	internal class DrawPathsScaled : AbstractScenario
	{
		public DrawPathsScaled() : base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			DrawScaledPaths(canvas);
		}

		private void DrawScaledPaths(ICanvas canvas)
		{
			PathF path = new(100, 100);
			path.AddArc(100, 100, 200, 200, 0, 180, true);
			path.LineTo(150, 100);
			path.Close();

			canvas.StrokeColor = Colors.Black;
			canvas.DrawPath(path);

			canvas.StrokeColor = Colors.Magenta;
			canvas.DrawPath(path.AsScaledPath(1.5f));

			canvas.StrokeColor = Colors.Green;
			canvas.DrawPath(path.AsScaledPath(0.5f, 1f));

			canvas.StrokeColor = Colors.Orange;
			canvas.DrawPath(path.AsScaledPath(1f, 0.5f));
		}
	}
}
