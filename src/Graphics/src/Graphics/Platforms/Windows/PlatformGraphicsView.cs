using System;
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

			float adjustedScaleX = 1f, adjustedScaleY = 1f;
			if (logicalWidth > 0 && logicalHeight > 0)
			{
				adjustedScaleX = actualWidth / logicalWidth;
				adjustedScaleY = actualHeight / logicalHeight;
				_dirty.Width = logicalWidth;
				_dirty.Height = logicalHeight;
			}
			else
			{
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
				// Apply adjusted scale via PlatformCanvas (not DrawingSession.Transform directly)
				// so the scale is tracked in CurrentState.Matrix, allowing any transforms applied
				// by the drawable to chain correctly on top of it. This maps the rounded logical
				// dimensions back to exact physical dimensions, filling the view edge-to-edge
				// without sub-pixel gaps. Uses epsilon to skip near-identity scales from
				// double-to-float precision loss.
				const float scaleEpsilon = 0.0001f;
				if (MathF.Abs(adjustedScaleX - 1f) > scaleEpsilon || MathF.Abs(adjustedScaleY - 1f) > scaleEpsilon)
				{
					_canvas.Scale(adjustedScaleX, adjustedScaleY);
				}
				_drawable.Draw(_canvas, _dirty);
			}
			finally
			{
				_canvas.ResetState();
				PlatformGraphicsService.ThreadLocalCreator = null;
			}
		}
	}
}
