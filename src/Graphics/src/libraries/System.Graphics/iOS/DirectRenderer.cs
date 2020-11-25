using System.Drawing;
using CoreGraphics;

namespace System.Graphics.CoreGraphics
{
    public class DirectRenderer : IGraphicsRenderer
    {
        private readonly NativeCanvas _canvas;
        private IDrawable _drawable;
        private NativeGraphicsView _graphicsView;

        public DirectRenderer()
        {
            _canvas = new NativeCanvas(() => CGColorSpace.CreateDeviceRGB());
        }

        public ICanvas Canvas => _canvas;

        public IDrawable Drawable
        {
            get => _drawable;
            set => _drawable = value;
        }

        public NativeGraphicsView GraphicsView
        {
            set => _graphicsView = value;
        }

        public void Draw(CGContext coreGraphics, RectangleF dirtyRect, bool inPanOrZoom)
        {
            _canvas.Context = coreGraphics;

            try
            {
                _drawable.Draw(_canvas, dirtyRect);
            }
            catch (Exception exc)
            {
                Logger.Error("An unexpected error occurred rendering the drawing.", exc);
            }
            finally
            {
                _canvas.Context = null;
            }
        }

        public void SizeChanged(float width, float height)
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
            _graphicsView?.SetNeedsDisplay();
        }

        public void Invalidate(float x, float y, float w, float h)
        {
            _graphicsView?.SetNeedsDisplayInRect(new Drawing.RectangleF(x, y, w, h));
        }
    }
}