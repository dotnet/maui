using System;
using System.Windows.Forms;

namespace Microsoft.Maui.Graphics.GDI
{
	public partial class GDIGraphicsView : UserControl
	{
		private readonly RectangleF dirtyRect = new RectangleF();
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
			renderer.Draw(e.Graphics, e.ClipRectangle.AsRectangleF());
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
