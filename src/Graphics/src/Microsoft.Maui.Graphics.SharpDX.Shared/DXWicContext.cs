using System;
using d2 = SharpDX.Direct2D1;
using dxgi = SharpDX.DXGI;
using wic = SharpDX.WIC;

namespace Microsoft.Maui.Graphics.SharpDX
{
    public class DXWicContext : IDisposable
    {
        private wic.Bitmap _bitmap;
        private DXCanvas _canvas;
        private d2.RenderTarget _renderTarget;

        public DXWicContext(int width, int height)
        {
            _bitmap = new wic.Bitmap(
                DXGraphicsService.FactoryImaging,
                width,
                height,
                wic.PixelFormat.Format32bppBGR,
                wic.BitmapCreateCacheOption.CacheOnLoad);
            var renderTargetProperties = new d2.RenderTargetProperties(d2.RenderTargetType.Default,
                new d2.PixelFormat(dxgi.Format.Unknown,
                    d2.AlphaMode.Unknown), 0, 0,
                d2.RenderTargetUsage.None,
                d2.FeatureLevel.Level_DEFAULT);
            _renderTarget = new d2.WicRenderTarget(DXGraphicsService.SharedFactory, _bitmap, renderTargetProperties);
            _renderTarget.BeginDraw();
            _canvas = new DXCanvas(_renderTarget);
        }

        public ICanvas Canvas => _canvas;

        public d2.RenderTarget RenderTarget => _renderTarget;

        public void Dispose()
        {
            if (_canvas != null)
            {
                _canvas.Dispose();
                _canvas = null;
            }

            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }

            if (_renderTarget != null)
            {
                _renderTarget.Dispose();
                _renderTarget = null;
            }
        }
    }
}