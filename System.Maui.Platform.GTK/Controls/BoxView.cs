using Cairo;
using System;
using System.Maui.Platform.GTK.Extensions;

namespace System.Maui.Platform.GTK.Controls
{
	public class BoxView : GtkFormsContainer
	{
		private Color _color;
		private int _height;
		private int _width;
		private int _topLeftRadius, _topRightRadius, _bottomLeftRadius, _bottomRightRadius;

		public void UpdateColor(Color color)
		{
			_color = color;
			QueueDraw();
		}

		public void ResetColor()
		{
			UpdateColor(Color.Default);
		}

		public void UpdateSize(int height, int width)
		{
			_height = height;
			_width = width;
			QueueDraw();
		}

		public void UpdateBorderRadius(int topLeftRadius = 5, int topRightRadius = 5, int bottomLeftRadius = 5, int bottomRightRadius = 5)
		{
			_topLeftRadius = topLeftRadius > 0 ? topLeftRadius : 0;
			_topRightRadius = topRightRadius > 0 ? topRightRadius : 0;
			_bottomLeftRadius = bottomLeftRadius > 0 ? bottomLeftRadius : 0;
			_bottomRightRadius = bottomRightRadius > 0 ? bottomRightRadius : 0;
			QueueDraw();
		}

		protected override void Draw(Gdk.Rectangle area, Context cr)
		{
			if (_color.IsDefaultOrTransparent())
			{
				return;
			}

			double radius = _topLeftRadius;
			int x = 0;
			int y = 0;
			int width = _width;
			int height = _height;

			cr.MoveTo(x, y);
			cr.Arc(x + width - _topRightRadius, y + _topRightRadius, _topRightRadius, Math.PI * 1.5, Math.PI * 2);
			cr.Arc(x + width - _bottomRightRadius, y + height - _bottomRightRadius, _bottomRightRadius, 0, Math.PI * .5);
			cr.Arc(x + _bottomLeftRadius, y + height - _bottomLeftRadius, _bottomLeftRadius, Math.PI * .5, Math.PI);
			cr.Arc(x + _topLeftRadius, y + _topLeftRadius, _topLeftRadius, Math.PI, Math.PI * 1.5);

			cr.SetSourceRGBA(_color.R, _color.G, _color.B, _color.A);

			cr.Fill();
		}
	}
}
