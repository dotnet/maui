using System.Windows;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace Microsoft.Maui.Graphics.Skia
{
    public class WDSkiaGraphicsView : SKElement
    {
        private RectangleF _dirtyRect;
        private IDrawable _drawable;
        private ISkiaGraphicsRenderer _renderer;

        public WDSkiaGraphicsView()
        {
            Renderer = CreateDefaultRenderer();
        }

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
                _renderer.SizeChanged((int) CanvasSize.Width, (int) CanvasSize.Height);
            }
        }

        private ISkiaGraphicsRenderer CreateDefaultRenderer()
        {
            return new WDSkiaDirectRenderer();
        }

        public Color BackgroundColor
        {
            get => _renderer.BackgroundColor;
            set => _renderer.BackgroundColor = value;
        }

        public IDrawable Drawable
        {
            get => _drawable;
            set
            {
                _drawable = value;
                _renderer.Drawable = _drawable;
            }
        }

        protected override void OnPaintSurface(
            SKPaintSurfaceEventArgs e)
        {
            _renderer?.Draw(e.Surface.Canvas, _dirtyRect);
        }

        protected override void OnRenderSizeChanged(
            SizeChangedInfo sizeInfo)
        {
            _dirtyRect.Width = (float) sizeInfo.NewSize.Width;
            _dirtyRect.Height = (float) sizeInfo.NewSize.Height;
            _renderer?.SizeChanged((int) sizeInfo.NewSize.Width, (int) sizeInfo.NewSize.Height);
        }

        public void Invalidate()
        {
            InvalidateVisual();
        }
    }
}
