using Microsoft.Graphics.Canvas.UI.Xaml;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	/// <summary>
	/// A Windows platform view that can be used to host drawings.
	/// </summary>
#if MAUI_GRAPHICS_WIN2D
	public sealed partial class W2DGraphicsView
#else
	public partial class PlatformGraphicsView
#endif
		: UserControl
	{
		private CanvasControl _canvasControl;
		private readonly PlatformCanvas _canvas;
		readonly ScalingCanvas _scalingCanvas;

		private IDrawable _drawable;
		private RectF _dirty;
		//private bool _resizeDrawable = true;

#if MAUI_GRAPHICS_WIN2D
		public W2DGraphicsView()
#else
		public PlatformGraphicsView()
#endif
		{
			_canvas = new PlatformCanvas();
			_scalingCanvas = new ScalingCanvas(_canvas);

			Loaded += UserControl_Loaded;
			Unloaded += UserControl_Unloaded;
		}

		public IDrawable Drawable
		{
			get => _drawable;
			set
			{
				_drawable = value;
				Invalidate();
			}
		}

		public void Invalidate()
		{
			_canvasControl?.Invalidate();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			_canvasControl = new CanvasControl();
			_canvasControl.Draw += OnDraw;
			Content = _canvasControl;
		}

		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			// Explicitly remove references to allow the Win2D controls to get garbage collected
			if (_canvasControl != null && !_canvasControl.IsLoaded)
			{
				_canvasControl.RemoveFromVisualTree();
				_canvasControl = null;
			}
		}

		private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			if (_drawable == null)
				return;

			var actualWidth = (float)sender.ActualWidth;
			var actualHeight = (float)sender.ActualHeight;

			var logicalWidth = MathF.Round(actualWidth);
			var logicalHeight = MathF.Round(actualHeight);

			float adjustedScaleX, adjustedScaleY;
			if (logicalWidth > 0 && logicalHeight > 0)
			{
				adjustedScaleX = actualWidth / logicalWidth;
				adjustedScaleY = actualHeight / logicalHeight;
				_dirty.Width = logicalWidth;
				_dirty.Height = logicalHeight;
			}
			else
			{
				adjustedScaleX = 1f;
				adjustedScaleY = 1f;
				_dirty.Width = actualWidth;
				_dirty.Height = actualHeight;
			}

			_dirty.X = 0f;
			_dirty.Y = 0f;

			PlatformGraphicsService.ThreadLocalCreator = sender;
			try
			{
				_canvas.Session = args.DrawingSession;
				_canvas.CanvasSize = new global::Windows.Foundation.Size(_dirty.Width, _dirty.Height);
				// Use adjusted scale factors that map rounded logical dimensions
				// back to exact physical dimensions, avoiding sub-pixel gaps at view edges.
				_scalingCanvas.ResetState();
				_scalingCanvas.Scale(adjustedScaleX, adjustedScaleY);
				_drawable.Draw(_scalingCanvas, _dirty);
			}
			finally
			{
				_canvas.ResetState();
				PlatformGraphicsService.ThreadLocalCreator = null;
			}
		}
	}
}
