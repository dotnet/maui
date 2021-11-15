using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Text.Immutable;
using Microsoft.Maui.Graphics.Text.Mutable;

namespace Maui.Controls.Sample
{
	public class TestWindowOverlay : WindowOverlay
	{
		IWindowOverlayElement testWindowDrawable;

		public TestWindowOverlay(IWindow window) 
			: base(window)
		{
			this.testWindowDrawable = new TestWindowDrawable(this);
			this.AddWindowElement(testWindowDrawable);
			this.EnableDrawableTouchHandling = true;
			this.OnTouch += TestWindowOverlay_OnTouch;
		}

		private void TestWindowOverlay_OnTouch(object sender, VisualDiagnosticsHitEvent e)
		{
			if (e.WindowOverlayElements.Contains(testWindowDrawable))
				System.Diagnostics.Debug.WriteLine("Greetings from VSCX!");
		}
	}

	public class TestWindowDrawable : IWindowOverlayElement
	{
		Circle _circle = new Circle(0, 0, 0);
		WindowOverlay overlay;

		public TestWindowDrawable(WindowOverlay overlay)
		{
			this.overlay = overlay;
		}

		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			canvas.FillColor = Color.FromRgba(225, 225, 255, 225);
			canvas.StrokeColor = Color.FromRgba(225, 225, 255, 225);
			var centerX = dirtyRect.Width - 50;
			var centerY = dirtyRect.Height - 50;
			_circle = new Circle(centerX, centerY, 40);
			canvas.FillCircle(centerX, centerY, 40);
		}

		public bool IsPointInElement(Point point)
		{
			return Circle.ContainsPoint(_circle, new Point(point.X / this.overlay.DPI, point.Y / this.overlay.DPI));
		}

		struct Circle
		{
			public float Radius;
			public PointF Center;

			public Circle(float x, float y, float r)
			{
				Radius = r;
				Center = new PointF(x, y);
			}

			public static bool ContainsPoint(Circle c, Point p)
			{
				return p.X <= c.Center.X + c.Radius
					&& p.X >= c.Center.X - c.Radius
					&& p.Y <= c.Center.Y + c.Radius
					&& p.Y >= c.Center.Y - c.Radius;
			}
		}
	}
}
