using Microsoft.Graphics.Canvas.UI.Xaml;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Microsoft.Maui.Graphics.Win2D
{
	public sealed class W2DGraphicsView : UserControl
	{
		private CanvasControl _canvasControl;
		private readonly W2DCanvas _canvas;

		private IDrawable _drawable;
		private RectF _dirty;
		//private bool _resizeDrawable = true;

		public W2DGraphicsView()
		{
			_canvas = new W2DCanvas();

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

			_dirty.X = 0f;
			_dirty.Y = 0f;
			_dirty.Width = (float)sender.ActualWidth;
			_dirty.Height = (float)sender.ActualHeight;

			W2DGraphicsService.ThreadLocalCreator = sender;
			_canvas.Session = args.DrawingSession;
			_canvas.CanvasSize = new global::Windows.Foundation.Size(_dirty.Width, _dirty.Height);
			_drawable.Draw(_canvas, _dirty);
			W2DGraphicsService.ThreadLocalCreator = null;
		}
	}
}
