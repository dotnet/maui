using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	internal class ScaleCanvas : AbstractScenario
	{
		public ScaleCanvas() : base(720, 1024)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			DrawTestShapesOnScaledCanvas(canvas, 1, 1, Colors.Black);
			DrawTestShapesOnScaledCanvas(canvas, 2, 2, Colors.Magenta);
			DrawTestShapesOnScaledCanvas(canvas, 2, 1, Colors.Green);
			DrawTestShapesOnScaledCanvas(canvas, 1, 2, Colors.Orange);
		}

		private void DrawTestShapesOnScaledCanvas(ICanvas canvas, float xScale, float yScale, Color color)
		{
			PathF path = new(100, 100);
			path.AddArc(100, 100, 200, 200, 0, 180, true);
			path.LineTo(150, 100);
			path.Close();

			canvas.SaveState();
			canvas.Scale(xScale, yScale);
			canvas.StrokeColor = color;
			canvas.StrokeSize = 2;
			canvas.DrawPath(path);
			canvas.RestoreState();
		}
	}
}
