using Gtk;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// A GTK drawing area that renders content using SkiaSharp with MAUI Graphics integration.
	/// </summary>
	public class GtkSkiaGraphicsView : SKDrawingArea
	{
		private RectF _dirtyRect;
		private IDrawable _drawable;
		private ISkiaGraphicsRenderer _renderer;

		/// <summary>
		/// Initializes a new instance of the <see cref="GtkSkiaGraphicsView"/> class.
		/// </summary>
		public GtkSkiaGraphicsView()
		{
			Renderer = CreateDefaultRenderer();
		}

		/// <summary>
		/// Gets or sets the Skia graphics renderer used to draw content on this view.
		/// </summary>
		/// <value>
		/// The Skia graphics renderer. If set to <c>null</c>, a default renderer will be created.
		/// </value>
		/// <remarks>
		/// When changing the renderer, the previous renderer will be disposed.
		/// </remarks>
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
				_renderer.SizeChanged((int)CanvasSize.Width, (int)CanvasSize.Height);
			}
		}

		/// <summary>
		/// Creates the default Skia graphics renderer.
		/// </summary>
		/// <returns>A new instance of <see cref="GtkSkiaDirectRenderer"/>.</returns>
		private ISkiaGraphicsRenderer CreateDefaultRenderer()
		{
			return new GtkSkiaDirectRenderer();
		}

		/// <summary>
		/// Gets or sets the background color of the view.
		/// </summary>
		/// <value>The color used to fill the background of the view.</value>
		public Color BackgroundColor
		{
			get => _renderer.BackgroundColor;
			set => _renderer.BackgroundColor = value;
		}

		/// <summary>
		/// Gets or sets the drawable content that will be rendered on this view.
		/// </summary>
		/// <value>The drawable content to be rendered.</value>
		public IDrawable Drawable
		{
			get => _drawable;
			set
			{
				_drawable = value;
				_renderer.Drawable = _drawable;
			}
		}

		/// <summary>
		/// Called when the drawing surface needs to be painted.
		/// </summary>
		/// <param name="e">The event arguments containing the surface to paint on.</param>
		protected override void OnPaintSurface(
			SKPaintSurfaceEventArgs e)
		{
			_renderer?.Draw(e.Surface.Canvas, _dirtyRect);
		}

		/// <summary>
		/// Called when the size of the drawing area is allocated or changed.
		/// </summary>
		/// <param name="allocation">The new allocation rectangle.</param>
		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			_dirtyRect.Width = (float)allocation.Width;
			_dirtyRect.Height = (float)allocation.Height;
			_renderer?.SizeChanged((int)allocation.Width, (int)allocation.Height);
			base.OnSizeAllocated(allocation);
		}

	}
}
