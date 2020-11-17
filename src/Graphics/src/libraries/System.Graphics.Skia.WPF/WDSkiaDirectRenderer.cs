using SkiaSharp;

namespace System.Graphics.Skia
{
    public class WDSkiaDirectRenderer : ISkiaGraphicsRenderer
    {
        private readonly SkiaCanvas _canvas;
        private readonly ScalingCanvas _scalingCanvas;
        private IDrawable _drawable;
        private WDSkiaGraphicsView _graphicsView;
        private Color _backgroundColor;

        public WDSkiaDirectRenderer()
        {
            _canvas = new SkiaCanvas();
            _scalingCanvas = new ScalingCanvas(_canvas);
        }

        public ICanvas Canvas => _scalingCanvas;

        public IDrawable Drawable
        {
            get => _drawable;
            set
            {
                _drawable = value;
                Invalidate();
            }
        }

        public WDSkiaGraphicsView GraphicsView
        {
            set => _graphicsView = value;
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public void Draw(
            SKCanvas skiaCanvas,
            RectangleF dirtyRect)
        {
            _canvas.Canvas = skiaCanvas;

            try
            {
                if (_backgroundColor != null)
                {
                    _canvas.FillColor = _backgroundColor;
                    _canvas.FillRectangle(dirtyRect);
                    _canvas.FillColor = Colors.White;
                }

                _drawable.Draw(_scalingCanvas, dirtyRect);
            }
            catch (Exception exc)
            {
                Logger.Error("An unexpected error occurred rendering the drawing.", exc);
            }
            finally
            {
                _canvas.Canvas = null;
                _scalingCanvas.ResetState();
            }
        }

        public void SizeChanged(
            int width,
            int height)
        {
            // Do nothing
        }

        public void Detached()
        {
            // Do nothing
        }

        public void Dispose()
        {
            // Do nothing
        }

        public void Invalidate()
        {
            _graphicsView?.Invalidate();
        }

        public void Invalidate(
            float x,
            float y,
            float w,
            float h)
        {
            _graphicsView?.Invalidate();
        }
    }
}