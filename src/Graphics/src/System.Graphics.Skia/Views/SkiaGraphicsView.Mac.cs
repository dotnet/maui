
using System.Graphics.CoreGraphics;
using System.Graphics.Skia;
using SkiaSharp.Views.Mac;

namespace System.Graphics.Skia.Views
{
    public class SkiaGraphicsView : SKCanvasView
    {
        private IDrawable _drawable;
        private SkiaCanvas _canvas;
        private ScalingCanvas _scalingCanvas;

        public SkiaGraphicsView(IDrawable drawable = null)
        {
            _canvas = new SkiaCanvas();
            _scalingCanvas = new ScalingCanvas(_canvas);
            Drawable = drawable;
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

        private void Invalidate()
        {
            if (Handle == IntPtr.Zero)
                return;

            NeedsDisplay = true;
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            if (_drawable == null) return;

            var skiaCanvas = e.Surface.Canvas;
            skiaCanvas.Clear();

            var scale = (float)Window.Screen.BackingScaleFactor;
            _canvas.Canvas = skiaCanvas;

            _scalingCanvas.ResetState();
            _scalingCanvas.Scale(scale, scale);
            _drawable.Draw(_scalingCanvas, Bounds.AsRectangleF());
        }
    }
}
