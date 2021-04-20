using Gtk;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace Microsoft.Maui.Graphics.Skia
{
	public class GtkSkiaGraphicsView : SKDrawingArea
	{
		private RectangleF _dirtyRect;
		private IDrawable _drawable;
		private ISkiaGraphicsRenderer _renderer;

		public GtkSkiaGraphicsView()
		{
			Renderer = CreateDefaultRenderer();
		}

		public ISkiaGraphicsRenderer Renderer
		{
			get => _renderer;

			set
			{
				if (_renderer != null)
				{
					_renderer.Drawable = null;
					_renderer.GraphicsView = null;
					_renderer.Dispose();
				}

				_renderer = value ?? CreateDefaultRenderer();
				_renderer.GraphicsView = this;
				_renderer.Drawable = _drawable;
				_renderer.SizeChanged((int) CanvasSize.Width, (int) CanvasSize.Height);
			}
		}

		private ISkiaGraphicsRenderer CreateDefaultRenderer()
		{
			return new GtkSkiaDirectRenderer();
		}

		public Color BackgroundColor
		{
			get => _renderer.BackgroundColor;
			set => _renderer.BackgroundColor = value;
		}

		public IDrawable Drawable
		{
			get => _drawable;
			set
			{
				_drawable = value;
				_renderer.Drawable = _drawable;
			}
		}

		protected override void OnPaintSurface(
			SKPaintSurfaceEventArgs e)
		{
			_renderer?.Draw(e.Surface.Canvas, _dirtyRect);
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation) {
			_dirtyRect.Width = (float) allocation.Width;
			_dirtyRect.Height = (float) allocation.Height;
			_renderer?.SizeChanged((int) allocation.Width, (int) allocation.Height);
			base.OnSizeAllocated (allocation);
		}

	}
}
