using System;
using System.Windows.Forms;

namespace Microsoft.Maui.Graphics.GDI
{
	public partial class GDIGraphicsView : UserControl
	{
		private readonly RectF dirtyRect = new RectF();
		private GDIGraphicsRenderer renderer;
		private IDrawable drawable;

		public GDIGraphicsView()
		{
			DoubleBuffered = true;
			Renderer = null;
			Drawable = null;
		}

		public GDIGraphicsView(IDrawable drawable = null, GDIGraphicsRenderer renderer = null)
		{
			DoubleBuffered = true;
			Drawable = drawable;
			Renderer = renderer;
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			// Do nothing
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			// Extend render area by 1px to prevent rendering artifacts at the edges
			RectF rect = new RectF(
				x: e.ClipRectangle.X - 1,
				y: e.ClipRectangle.Y - 1,
				width: e.ClipRectangle.Width + 2,
				height: e.ClipRectangle.Height + 2);

			renderer.Draw(e.Graphics, rect);
		}

		public bool Dirty
		{
			get => renderer.Dirty;
			set => renderer.Dirty = value;
		}

		public Color BackgroundColor
		{
			get => renderer.BackgroundColor;
			set => renderer.BackgroundColor = value;
		}

		public GDIGraphicsRenderer Renderer
		{
			get => renderer;

			set
			{
				if (renderer != null)
				{
					renderer.Drawable = null;
					renderer.GraphicsView = null;
					renderer.Dispose();
				}

				renderer = value ?? new GDIDirectGraphicsRenderer()
				{
					BackgroundColor = Colors.White
				};

				renderer.GraphicsView = this;
				renderer.Drawable = drawable;
			}
		}

		public IDrawable Drawable
		{
			get => drawable;
			set
			{
				drawable = value;
				if (renderer != null)
				{
					renderer.Drawable = drawable;
				}
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			renderer.SizeChanged(Width, Height);
		}
	}
}
