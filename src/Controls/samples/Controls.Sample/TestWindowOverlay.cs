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
			testWindowDrawable = new TestWindowDrawable(this);
			AddWindowElement(testWindowDrawable);
			EnableDrawableTouchHandling = true;
			Tapped += OnTapped;
		}

		async void OnTapped(object sender, WindowOverlayTappedEventArgs e)
		{
			if (e.WindowOverlayElements.Contains(testWindowDrawable))
			{
				var result = await XamlApp.Current.MainPage.DisplayActionSheet("Greetings from Visual Studio Client Experiences!", "Goodbye!", null, "Do something", "Do something else", "Do something... with feeling.");
				System.Diagnostics.Debug.WriteLine(result);
			}
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
			canvas.FillColor = Color.FromRgba(255, 0, 0, 225);
			canvas.StrokeColor = Color.FromRgba(225, 0, 0, 225);
			canvas.FontColor = Colors.Orange;
			canvas.FontSize = 40f;
			var centerX = dirtyRect.Width - 50;
			var centerY = dirtyRect.Height - 50;
			_circle = new Circle(centerX, centerY, 40);
			canvas.FillCircle(centerX, centerY, 40);
			canvas.DrawString($"🔥", centerX, centerY + 10, HorizontalAlignment.Center);
		}

		public bool Contains(Point point)
		{
			return Circle.ContainsPoint(_circle, new Point(point.X / this.overlay.Density, point.Y / this.overlay.Density));
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
