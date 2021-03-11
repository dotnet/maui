using Microsoft.Maui.Graphics.Android;
using Microsoft.Maui.Graphics.Skia;
using Android.App;
using Android.Content;
using Android.Util;
using SkiaSharp.Views.Android;

namespace Microsoft.Maui.Graphics.Skia.Views
{
    public class SkiaGraphicsView : SKCanvasView
    {
        private IDrawable _drawable;
        private SkiaCanvas _canvas;
        private ScalingCanvas _scalingCanvas;
        private float _width, _height;
        private readonly float _scale = 1;

        public SkiaGraphicsView(Context context, IDrawable drawable = null) : base( context)
        {
            _scale = Resources.DisplayMetrics.Density;
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

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            if (_drawable == null) return;

            var skiaCanvas = e.Surface.Canvas;
            skiaCanvas.Clear();

            _canvas.Canvas = skiaCanvas;

            _scalingCanvas.ResetState();
            _scalingCanvas.Scale(_scale, _scale);
           
            _drawable.Draw(_scalingCanvas, new RectangleF(0,0,_width / _scale, _height / _scale));
        }

        protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
        {
            base.OnSizeChanged(width, height, oldWidth, oldHeight);
            _width = width;
            _height = height;
        }
    }
}
