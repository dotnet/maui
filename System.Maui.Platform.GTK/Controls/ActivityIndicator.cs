using Cairo;
using System;

namespace Xamarin.Forms.Platform.GTK.Controls
{
	public class ActivityIndicator : GtkFormsContainer
	{
		private const int DefaultHeight = 48;
		private const int DefaultWidth = 48;
		private const int Lines = 8;
		private bool _running;
		private int _current;
		private Color _color;

		public ActivityIndicator()
		{
			HeightRequest = DefaultHeight;
			WidthRequest = DefaultWidth;
		}

		public void Start()
		{
			_running = true;
			GLib.Timeout.Add(100, ExposeTimeoutHandler);	// Every 100 ms.
			QueueDraw();
		}

		public void Stop()
		{
			_running = false;
			QueueDraw();
		}

		public void UpdateColor(Color color)
		{
            _color = color;
		}

		private bool ExposeTimeoutHandler()
		{
			if (_current + 1 > Lines)
			{
				_current = 0;
			}
			else
			{
				_current++;
			}
			QueueDraw();
			return (_running);
		}

		protected override void Draw(Gdk.Rectangle area, Context cr)
		{
			// Draw Activity Indicator
			double radius;
			double half;
			double x, y;

			x = Allocation.Width / 2;
			y = Allocation.Height / 2;

			radius = Math.Min(x, y);

			half = Lines / 2;

			for (int i = 0; i < Lines; i++)
			{
				double move = (double)((i + Lines - _current) % Lines) / Lines;
				double inset = 0.5 * radius;

				cr.Save();
				cr.SetSourceRGBA(_color.R, _color.G, _color.B, move);
				cr.LineWidth *= 2;
				cr.MoveTo(move + x + (radius - inset) * Math.Cos(i * Math.PI / half),
						  move + y + (radius - inset) * Math.Sin(i * Math.PI / half));
				cr.LineTo(x + radius * Math.Cos(i * Math.PI / half),
						  y + radius * Math.Sin(i * Math.PI / half));

				cr.Stroke();
				cr.Restore();
			}
		}
	}
}
