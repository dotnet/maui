using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public class TestWindowOverlay : WindowOverlay
	{
		IWindowOverlayElement _testWindowDrawable;

		public TestWindowOverlay(Window window)
			: base(window)
		{
			_testWindowDrawable = new TestOverlayElement(this);

			AddWindowElement(_testWindowDrawable);

			EnableDrawableTouchHandling = true;
			Tapped += OnTapped;
		}

		async void OnTapped(object? sender, WindowOverlayTappedEventArgs e)
		{
			if (!e.WindowOverlayElements.Contains(_testWindowDrawable))
				return;

			var window = Application.Current!.Windows.FirstOrDefault(w => w == Window);

			System.Diagnostics.Debug.WriteLine($"Tapped the test overlay button.");

			var result = await window!.Page!.DisplayActionSheetAsync(
				"Greetings from Visual Studio Client Experiences!",
				"Goodbye!",
				null,
				"Do something", "Do something else", "Do something... with feeling.");

			System.Diagnostics.Debug.WriteLine(result);
		}

		class TestOverlayElement : IWindowOverlayElement
		{
			readonly WindowOverlay _overlay;
			Circle _circle = new Circle(0, 0, 0);

			public TestOverlayElement(WindowOverlay overlay)
			{
				_overlay = overlay;
			}

			public void Draw(ICanvas canvas, RectF dirtyRect)
			{
				canvas.FillColor = Color.FromRgba(255, 0, 0, 225);
				canvas.StrokeColor = Color.FromRgba(225, 0, 0, 225);
				canvas.FontColor = Colors.Orange;
				canvas.FontSize = 40f;

				var centerX = dirtyRect.Width - 50;
				var centerY = dirtyRect.Height - 50;
				_circle = new Circle(centerX, centerY, 40);

				canvas.FillCircle(centerX, centerY, 40);
				canvas.DrawString("🔥", centerX, centerY + 10, HorizontalAlignment.Center);
			}

			public bool Contains(Point point) =>
				_circle.ContainsPoint(new Point(point.X / _overlay.Density, point.Y / _overlay.Density));

			struct Circle
			{
				public float Radius;
				public PointF Center;

				public Circle(float x, float y, float r)
				{
					Radius = r;
					Center = new PointF(x, y);
				}

				public bool ContainsPoint(Point p) =>
					p.X <= Center.X + Radius &&
					p.X >= Center.X - Radius &&
					p.Y <= Center.Y + Radius &&
					p.Y >= Center.Y - Radius;
			}
		}
	}
}