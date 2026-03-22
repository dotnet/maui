using System.Windows;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// A WPF control that renders graphics using SkiaSharp.
	/// </summary>
	/// <remarks>
	/// This control extends SKElement to provide .NET MAUI graphics rendering capabilities in WPF applications.
	/// </remarks>
	public class WDSkiaGraphicsView : SKElement
	{
		private RectF _dirtyRect;
		private IDrawable _drawable;
		private ISkiaGraphicsRenderer _renderer;

		/// <summary>
		/// Initializes a new instance of the <see cref="WDSkiaGraphicsView"/> class.
		/// </summary>
		public WDSkiaGraphicsView()
		{
			Renderer = CreateDefaultRenderer();
		}

		/// <summary>
		/// Gets or sets the renderer used to handle drawing operations.
		/// </summary>
		/// <remarks>
		/// When a new renderer is set, the previous renderer is disposed, and the new renderer is 
		/// configured with the current drawable and view dimensions.
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
		/// Creates the default renderer used by this view.
		/// </summary>
		/// <returns>A new instance of <see cref="WDSkiaDirectRenderer"/>.</returns>
		private ISkiaGraphicsRenderer CreateDefaultRenderer()
		{
			return new WDSkiaDirectRenderer();
		}

		/// <summary>
		/// Gets or sets the background color of the graphics view.
		/// </summary>
		/// <remarks>
		/// This property delegates to the underlying renderer's background color property.
		/// </remarks>
		public Color BackgroundColor
		{
			get => _renderer.BackgroundColor;
			set => _renderer.BackgroundColor = value;
		}

		/// <summary>
		/// Gets or sets the drawable that provides the content to render.
		/// </summary>
		/// <remarks>
		/// When this property is set, the same drawable is passed to the underlying renderer.
		/// </remarks>
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
		/// Called when the surface needs to be painted.
		/// </summary>
		/// <param name="e">The event arguments containing the surface to paint on.</param>
		protected override void OnPaintSurface(
			SKPaintSurfaceEventArgs e)
		{
			_renderer?.Draw(e.Surface.Canvas, _dirtyRect);
		}

		/// <summary>
		/// Called when the render size of the element has changed.
		/// </summary>
		/// <param name="sizeInfo">Information about the size change.</param>
		protected override void OnRenderSizeChanged(
			SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			_dirtyRect.Width = (float)sizeInfo.NewSize.Width;
			_dirtyRect.Height = (float)sizeInfo.NewSize.Height;
			_renderer?.SizeChanged((int)sizeInfo.NewSize.Width, (int)sizeInfo.NewSize.Height);
		}

		/// <summary>
		/// Invalidates the entire view, causing it to be redrawn.
		/// </summary>
		public void Invalidate()
		{
			InvalidateVisual();
		}
	}
}
