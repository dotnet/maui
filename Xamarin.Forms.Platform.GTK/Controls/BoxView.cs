using Cairo;
using Gdk;
using Gtk;
using System;

namespace Xamarin.Forms.Platform.GTK.Controls
{
	public class BoxView : VBox
	{
		private BoxViewDrawingArea _boxView;

		public BoxView()
		{
			// Custom control created with Cairo. It gives nice possibilities as rounded corners, etc.
			_boxView = new BoxViewDrawingArea();

			Add(_boxView);
		}

		public void UpdateColor(Gdk.Color color)
		{
			if (_boxView != null)
			{
				_boxView.Color = color;
			}
		}

		public void ResetColor()
		{
			UpdateColor(Gdk.Color.Zero);
		}

		public void UpdateBackgroundColor(Gdk.Color color)
		{
			if (_boxView != null)
			{
				_boxView.ModifyBg(StateType.Normal, color);
			}
		}

		public void UpdateSize(int height, int width)
		{
			if (_boxView != null)
			{
				_boxView.Height = height;
				_boxView.Width = width;
			}
		}

		public void UpdateBorderRadius(int topLeftRadius = 5, int topRightRadius = 5, int bottomLeftRadius = 5, int bottomRightRadius = 5)
		{
			if (_boxView != null)
			{
				_boxView.TopLeftRadius = topLeftRadius > 0 ? topLeftRadius : 0;
				_boxView.TopRightRadius = topRightRadius > 0 ? topRightRadius : 0;
				_boxView.BottomLeftRadius = bottomLeftRadius > 0 ? bottomLeftRadius : 0;
				_boxView.BottomRightRadius = bottomRightRadius > 0 ? bottomRightRadius : 0;
			}
		}
	}

	public class BoxViewDrawingArea : DrawingArea
	{
		private Context _context;
		private EventExpose _event;
		private Gdk.Color _color;
		private int _height;
		private int _width;
		private int _topLeftRadius, _topRightRadius, _bottomLeftRadius, _bottomRightRadius;

		public BoxViewDrawingArea()
		{
			QueueResize();
		}

		public Gdk.Color Color
		{
			get { return (_color); }
			set
			{
				_color = value;
				QueueDraw();
			}
		}

		public int Height
		{
			get { return (_height); }
			set
			{
				_height = value;
				QueueDraw();
			}
		}
		public int Width
		{
			get { return (_width); }
			set
			{
				_width = value;
				QueueDraw();
			}
		}

		public int TopLeftRadius
		{
			get { return (_topLeftRadius); }
			set
			{
				_topLeftRadius = value;
				QueueDraw();
			}
		}

		public int TopRightRadius
		{
			get { return (_topRightRadius); }
			set
			{
				_topRightRadius = value;
				QueueDraw();
			}
		}

		public int BottomLeftRadius
		{
			get { return (_bottomLeftRadius); }
			set
			{
				_bottomLeftRadius = value;
				QueueDraw();
			}
		}

		public int BottomRightRadius
		{
			get { return (_bottomRightRadius); }
			set
			{
				_bottomRightRadius = value;
				QueueDraw();
			}
		}

		protected override bool OnExposeEvent(EventExpose ev)
		{
			using (var cr = CairoHelper.Create(GdkWindow))
			{
				Draw(cr, ev);
			}

			return (true);
		}

		private void Draw(Context cr, EventExpose ev)
		{
			_context = cr;
			_event = ev;

			DrawBoxView(_context, _event);
		}

		private void DrawBoxView(Context cr, EventExpose ev)
		{
			if (Color.Equal(Gdk.Color.Zero)) return;

			int clipHeight = ev.Area.Height > 0 ? Height : 0;
			int clipWidth = ev.Area.Width > 0 ? Width : 0;

			double radius = TopLeftRadius;
			int x = 0;
			int y = 0;
			int width = Width;
			int height = Height;

			cr.Rectangle(
				radius,
				0,
				clipHeight,
				clipWidth);

			cr.MoveTo(x, y);
			cr.Arc(x + width - TopRightRadius, y + TopRightRadius, TopRightRadius, Math.PI * 1.5, Math.PI * 2);
			cr.Arc(x + width - BottomRightRadius, y + height - BottomRightRadius, BottomRightRadius, 0, Math.PI * .5);
			cr.Arc(x + BottomLeftRadius, y + height - BottomLeftRadius, BottomLeftRadius, Math.PI * .5, Math.PI);
			cr.Arc(x + TopLeftRadius, y + TopLeftRadius, TopLeftRadius, Math.PI, Math.PI * 1.5);

			var cairoColor = new Cairo.Color(
			   (double)Color.Red / ushort.MaxValue,
			   (double)Color.Green / ushort.MaxValue,
			   (double)Color.Blue / ushort.MaxValue);

			cr.SetSourceRGB(cairoColor.R, cairoColor.G, cairoColor.B);

			cr.Fill();
		}
	}
}