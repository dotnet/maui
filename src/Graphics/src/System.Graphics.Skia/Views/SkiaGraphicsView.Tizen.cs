using System.Graphics;
using System.Graphics.Skia;
using SkiaSharp.Views.Tizen;
using ElmSharp;

namespace System.Graphics.Skia.Views
{
    public class SkiaGraphicsView : SKCanvasView
    {
        private IDrawable _drawable;
        private SkiaCanvas _canvas;
        private ScalingCanvas _scalingCanvas;

        public SkiaGraphicsView(EvasObject parent, IDrawable drawable = null) : base(parent)
        {
            _canvas = new SkiaCanvas();
            _scalingCanvas = new ScalingCanvas(_canvas);
            Drawable = drawable;
            PaintSurface += OnPaintSurface;
            IgnorePixelScaling = true;
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

        protected virtual void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (_drawable == null) return;

            var skiaCanvas = e.Surface.Canvas;
            skiaCanvas.Clear();

            _canvas.Canvas = skiaCanvas;
            _drawable.Draw(_scalingCanvas, new RectangleF(0, 0, GetSurfaceSize().Width, GetSurfaceSize().Height));
        }
    }
}
