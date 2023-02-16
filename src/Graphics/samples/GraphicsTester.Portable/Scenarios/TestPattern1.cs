using Microsoft.Maui.Graphics;

namespace GraphicsTester.Scenarios
{
	public class TestPattern1 : AbstractScenario
	{
		public TestPattern1() : base(2000, 1500)
		{
		}

		public override void Draw(ICanvas canvas)
		{
			canvas.SaveState();

			//
			// DrawXXXX Methods
			//
			canvas.StrokeColor = Colors.Grey;
			canvas.DrawLine(0, 0, 100, 100);
			canvas.DrawRectangle(100, 0, 100, 100);
			canvas.DrawEllipse(200, 0, 100, 100);
			canvas.DrawRoundedRectangle(300, 0, 100, 100, 25);

			var path = new PathF();
			path.MoveTo(400, 0);
			path.LineTo(400, 100);
			path.QuadTo(500, 100, 500, 0);
			path.CurveTo(450, 0, 500, 50, 450, 50);
			canvas.DrawPath(path);

			canvas.DrawRectangle(500, 0, 100, 50);
			canvas.DrawEllipse(600, 0, 100, 50);
			canvas.DrawRoundedRectangle(700, 0, 100, 50, 25);
			canvas.DrawRoundedRectangle(800, 0, 100, 25, 25);

			//
			// FillXXXX Methods
			//

			canvas.FillColor = Colors.Red;
			canvas.FillRectangle(210, 210, 80, 80);

			canvas.FillColor = Colors.Green;
			canvas.FillEllipse(310, 210, 80, 80);

			canvas.FillColor = Colors.Blue;
			canvas.FillRoundedRectangle(410, 210, 80, 80, 10);

			canvas.FillColor = Colors.CornflowerBlue;
			path = new PathF();
			path.MoveTo(510, 210);
			path.LineTo(550, 290);
			path.LineTo(590, 210);
			path.Close();
			canvas.FillPath(path);

			canvas.FillColor = Colors.White;

			//
			//StrokeLocation.CENTER
			//
			canvas.StrokeSize = 10;
			canvas.DrawRectangle(50, 400, 100, 50);
			canvas.DrawEllipse(200, 400, 100, 50);
			canvas.DrawRoundedRectangle(350, 400, 100, 50, 25);

			path = new PathF();
			path.MoveTo(550, 400);
			path.LineTo(500, 450);
			path.LineTo(600, 450);
			path.Close();
			canvas.DrawPath(path);

			//
			// Stroke Color and Line Caps
			//

			canvas.StrokeColor = Colors.CornflowerBlue;
			canvas.DrawLine(100, 120, 300, 120);

			canvas.StrokeColor = Colors.Red;
			canvas.StrokeLineCap = LineCap.Butt;
			canvas.DrawLine(100, 140, 300, 140);

			canvas.StrokeColor = Colors.Green;
			canvas.StrokeLineCap = LineCap.Round;
			canvas.DrawLine(100, 160, 300, 160);

			canvas.StrokeColor = Colors.Blue;
			canvas.StrokeLineCap = LineCap.Square;
			canvas.DrawLine(100, 180, 300, 180);

			canvas.StrokeLineCap = LineCap.Butt;

			//
			// Line Joins
			//

			canvas.StrokeColor = Colors.Black;

			path = new PathF();
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

			canvas.MiterLimit = CanvasDefaults.DefaultMiterLimit;

			//
			// Stroke Dash Pattern
			//
			canvas.StrokeSize = 1;
			canvas.StrokeDashPattern = DOTTED;
			canvas.DrawLine(650, 120, 800, 120);

			canvas.StrokeSize = 3;
			canvas.StrokeDashPattern = DOTTED;
			canvas.DrawLine(650, 140, 800, 140);

			canvas.StrokeDashPattern = DASHED_DOT;
			canvas.DrawLine(650, 160, 800, 160);

			canvas.StrokeDashPattern = SOLID;
			canvas.DrawLine(650, 180, 800, 180);

			canvas.StrokeLineCap = LineCap.Butt;

			//
			// Linear Gradient Fill
			//

			var linearGradientPaint = new LinearGradientPaint
			{
				StartColor = Colors.White,
				EndColor = Colors.Black
			};

			linearGradientPaint.StartPoint = new Point(0.1, 0.1);
			linearGradientPaint.EndPoint = new Point(0.9, 0.9);

			var linearRectangleRectangle = new RectF(50, 700, 100, 50);
			canvas.SetFillPaint(linearGradientPaint, linearRectangleRectangle);
			canvas.FillRectangle(linearRectangleRectangle);

			linearGradientPaint.StartPoint = new Point(0.1, 0.1);
			linearGradientPaint.EndPoint = new Point(0.9, 0.9);

			var linearEllipseRectangle = new RectF(200, 700, 100, 50);
			canvas.SetFillPaint(linearGradientPaint, linearEllipseRectangle);
			canvas.FillEllipse(linearEllipseRectangle);

			linearGradientPaint.AddOffset(.5f, Colors.IndianRed);
			linearGradientPaint.StartPoint = new Point(0.1, 0.1);
			linearGradientPaint.EndPoint = new Point(0.9, 0.9);

			var linearRoundedRectangleRectangle = new RectF(350, 700, 100, 50);
			canvas.SetFillPaint(linearGradientPaint, linearRoundedRectangleRectangle);
			canvas.FillRoundedRectangle(linearRoundedRectangleRectangle, 25);

			path = new PathF();
			path.MoveTo(550, 700);
			path.LineTo(500, 750);
			path.LineTo(600, 750);
			path.Close();

			linearGradientPaint.StartPoint = new Point(0.1, 0.1);
			linearGradientPaint.EndPoint = new Point(0.9, 0.9);

			var linearPathRectangle = new RectF(500, 700, 200, 50);
			canvas.SetFillPaint(linearGradientPaint, linearPathRectangle);
			canvas.FillPath(path);

			//
			// Radial Gradient Fill
			//

			var radialGradientPaint = new RadialGradientPaint
			{
				StartColor = Colors.White,
				EndColor = Colors.Black
			};

			radialGradientPaint.Center = new Point(0.5, 0.5);
			radialGradientPaint.Radius = 0.5;

			var radialRectangleRectangle = new RectF(50, 800, 100, 50);
			canvas.SetFillPaint(radialGradientPaint, radialRectangleRectangle);
			canvas.FillRectangle(radialRectangleRectangle);

			radialGradientPaint.Center = new Point(0.5, 0.5);
			radialGradientPaint.Radius = 0.5;

			var radialEllipseRectangle = new RectF(200, 800, 100, 50);
			canvas.SetFillPaint(radialGradientPaint, radialEllipseRectangle);
			canvas.FillEllipse(radialEllipseRectangle);

			radialGradientPaint.AddOffset(.5f, Colors.IndianRed);
			radialGradientPaint.Center = new Point(0.5, 0.5);
			radialGradientPaint.Radius = 0.5;

			var radialRoundedRectangleRectangle = new RectF(350, 800, 100, 50);
			canvas.SetFillPaint(radialGradientPaint, radialRoundedRectangleRectangle);
			canvas.FillRoundedRectangle(radialRoundedRectangleRectangle, 25);

			path = new PathF();
			path.MoveTo(550, 800);
			path.LineTo(500, 850);
			path.LineTo(600, 850);
			path.Close();

			radialGradientPaint.Center = new Point(0.5, 0.5);
			radialGradientPaint.Radius = 0.5;

			var radialPathRectangle = new RectF(550, 800, 200, 50);
			canvas.SetFillPaint(radialGradientPaint, radialPathRectangle);
			canvas.FillPath(path);

			//
			// Solid Fill With Shadow
			//

			canvas.SaveState();
			canvas.FillColor = Colors.CornflowerBlue;
			canvas.SetShadow(new SizeF(5, 5), 0, Colors.Grey);
			canvas.FillRectangle(50, 900, 100, 50);

			canvas.SetShadow(new SizeF(5, 5), 2, Colors.Red);
			canvas.FillEllipse(200, 900, 100, 50);

			canvas.SetShadow(new SizeF(5, 5), 5, Colors.Green);
			canvas.FillRoundedRectangle(350, 900, 100, 50, 25);

			canvas.SetShadow(new SizeF(10, 10), 5, Colors.Blue);

			path = new PathF();
			path.MoveTo(550, 900);
			path.LineTo(500, 950);
			path.LineTo(600, 950);
			path.Close();

			canvas.FillPath(path);

			//
			// Draw With Shadow
			//

			canvas.StrokeColor = Colors.Black;
			canvas.SetShadow(new SizeF(5, 5), 0, Colors.Grey);
			canvas.DrawRectangle(50, 1000, 100, 50);

			canvas.SetShadow(new SizeF(5, 5), 2, Colors.Red);
			canvas.DrawEllipse(200, 1000, 100, 50);

			canvas.SetShadow(new SizeF(5, 5), 5, Colors.Green);
			canvas.DrawRoundedRectangle(350, 1000, 100, 50, 25);

			canvas.SetShadow(new SizeF(10, 10), 5, Colors.Blue);
			path = new PathF();
			path.MoveTo(550, 1000);
			path.LineTo(500, 1050);
			path.LineTo(600, 1050);
			path.Close();

			canvas.DrawPath(path);

			canvas.RestoreState();

			//
			// Solid Fill Without Shadow
			//

			canvas.FillColor = Colors.DarkOliveGreen;
			canvas.FillRectangle(50, 1100, 100, 50);
			canvas.FillEllipse(200, 1100, 100, 50);
			canvas.FillRoundedRectangle(350, 1100, 100, 50, 25);

			path = new PathF();
			path.MoveTo(550, 1100);
			path.LineTo(500, 1150);
			path.LineTo(600, 1150);
			path.Close();

			canvas.FillPath(path);

			//
			// FILL WITH SHADOW USING ALPHA
			//

			canvas.SaveState();

			canvas.Alpha = .25f;
			canvas.FillColor = Colors.CornflowerBlue;
			canvas.SetShadow(new SizeF(5, 5), 0, Colors.Grey);
			canvas.FillRectangle(50, 1200, 100, 50);

			canvas.Alpha = .5f;
			canvas.SetShadow(new SizeF(5, 5), 2, Colors.Red);
			canvas.FillEllipse(200, 1200, 100, 50);

			canvas.Alpha = .75f;
			canvas.SetShadow(new SizeF(5, 5), 5, Colors.Green);
			canvas.FillRoundedRectangle(350, 1200, 100, 50, 25);

			canvas.Alpha = 1;
			canvas.SetShadow(new SizeF(10, 10), 5, Colors.Blue);

			path = new PathF();
			path.MoveTo(550, 1200);
			path.LineTo(500, 1250);
			path.LineTo(600, 1250);
			path.Close();

			canvas.FillPath(path);
			canvas.RestoreState();

			//
			// Test Scaling
			//

			canvas.StrokeSize = 1;
			canvas.StrokeColor = Colors.Black;
			canvas.DrawLine(10, 0, 0, 10);

			canvas.SaveState();

			canvas.Scale(2, 2);
			canvas.DrawLine(10, 0, 0, 10);

			canvas.Scale(2, 2);
			canvas.DrawLine(10, 0, 0, 10);

			canvas.RestoreState();

			//
			// Test simple rotation relative to 0,0
			//

			canvas.SaveState();
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

			canvas.DrawRectangle(60, 60, 10, 10);

			//
			// Test rotation relative to a point
			//

			canvas.DrawRectangle(25, 125, 50, 50);

			canvas.SaveState();
			canvas.Rotate(5, 50, 150);
			canvas.DrawRectangle(25, 125, 50, 50);
			canvas.RestoreState();

			canvas.SaveState();
			canvas.Rotate(-5, 50, 150);
			canvas.DrawRectangle(25, 125, 50, 50);
			canvas.RestoreState();

			//
			// Test text
			//

			canvas.StrokeSize = 1;
			canvas.StrokeColor = Colors.Blue;

			const string vTextLong =
				"Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
			const string vTextShort = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. ";

			for (int x = 0; x < 4; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					float dx = 1000 + x * 200;
					float dy = 0 + y * 150;

					canvas.DrawRectangle(dx, dy, 190, 140);

					var vHorizontalAlignment = (HorizontalAlignment)x;
					var vVerticalAlignment = (VerticalAlignment)y;

					canvas.Font = Font.Default;
					canvas.FontSize = 12f;
					canvas.DrawString(vTextLong, dx, dy, 190, 140, vHorizontalAlignment, vVerticalAlignment);
				}
			}

			canvas.SaveState();
			canvas.SetShadow(new SizeF(2, 2), 2, Colors.DarkGrey);

			for (int x = 0; x < 4; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					float dx = 1000 + x * 200;
					float dy = 450 + y * 150;

					canvas.DrawRectangle(dx, dy, 190, 140);

					var vHorizontalAlignment = (HorizontalAlignment)x;
					var vVerticalAlignment = (VerticalAlignment)y;

					canvas.Font = Font.Default;
					canvas.FontSize = 12f;
					canvas.DrawString(vTextShort, dx, dy, 190, 140, vHorizontalAlignment, vVerticalAlignment);
				}
			}

			canvas.RestoreState();

			for (int y = 0; y < 3; y++)
			{
				float dx = 1000 + y * 200;
				const float dy = 1050;

				canvas.DrawRectangle(dx, dy, 190, 140);

				const HorizontalAlignment vHorizontalAlignment = HorizontalAlignment.Left;
				var vVerticalAlignment = (VerticalAlignment)y;

				canvas.Font = Font.Default;
				canvas.FontSize = 12f;
				canvas.DrawString(
					vTextLong,
					dx,
					dy,
					190,
					140,
					vHorizontalAlignment,
					vVerticalAlignment,
					TextFlow.OverflowBounds);
			}

			//
			// Test simple drawing string
			//
			canvas.DrawLine(1000, 1300, 1200, 1300);
			canvas.DrawLine(1000, 1325, 1200, 1325);
			canvas.DrawLine(1000, 1350, 1200, 1350);
			canvas.DrawLine(1000, 1375, 1200, 1375);
			canvas.DrawLine(1100, 1300, 1100, 1400);
			canvas.DrawString("This is a test.", 1100, 1300, HorizontalAlignment.Left);
			canvas.DrawString("This is a test.", 1100, 1325, HorizontalAlignment.Center);
			canvas.DrawString("This is a test.", 1100, 1350, HorizontalAlignment.Right);
			canvas.DrawString("This is a test.", 1100, 1375, HorizontalAlignment.Justified);

			//
			// Test inverse clipping area
			//

			canvas.SaveState();
			canvas.DrawRectangle(200, 1300, 200, 50);
			canvas.SubtractFromClip(200, 1300, 200, 50);
			canvas.DrawLine(100, 1325, 500, 1325);
			canvas.DrawLine(300, 1275, 300, 1375);
			canvas.RestoreState();

			//
			// Test String Measuring
			//

			canvas.StrokeColor = Colors.Blue;
			for (int i = 0; i < 4; i++)
			{
				canvas.FontSize = 12 + i * 6;
				canvas.DrawString("Test String Length", 650, 400 + (100 * i), HorizontalAlignment.Left);

				var size = canvas.GetStringSize("Test String Length", Font.Default, 12 + i * 6);
				canvas.DrawRectangle(650, 400 + (100 * i), size.Width, size.Height);
			}

			//
			// Test Path Measuring
			//

			var vBuilder = new PathBuilder();
			path =
				vBuilder.BuildPath(
					"M0 52.5 C60 -17.5 60 -17.5 100 52.5 C140 122.5 140 122.5 100 152.5 Q60 182.5 0 152.5 Z");

			canvas.SaveState();
			canvas.Translate(650, 900);
			canvas.StrokeColor = Colors.Black;
			canvas.DrawPath(path);

			canvas.RestoreState();
		}
	}
}
