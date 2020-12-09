using Android.Content;
using Android.Graphics;

namespace System.Graphics.Android
{
    public class DirectRenderer : IGraphicsRenderer
    {
        private readonly NativeCanvas _canvas;
        private readonly ScalingCanvas _scalingCanvas;
        private IDrawable _drawable;
        private GraphicsView _graphicsView;
        private Color _backgroundColor;

        public DirectRenderer(Context context)
        {
            _canvas = new NativeCanvas(context);
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

        public GraphicsView GraphicsView
        {
            set => _graphicsView = value;
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public void Draw(Canvas androidCanvas, RectangleF dirtyRect)
        {
            _canvas.Canvas = androidCanvas;

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

        public void SizeChanged(int width, int height)
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

        public void Invalidate(float x, float y, float w, float h)
        {
            _graphicsView?.Invalidate();
        }
    }
}