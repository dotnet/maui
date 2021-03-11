using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Factory = SharpDX.Direct2D1.Factory;

namespace Microsoft.Maui.Graphics.SharpDX.WindowsForms
{
    public class WFDirectGraphicsRenderer : IGraphicsRenderer
    {
        public Factory Factory2D { get; private set; }

        public global::SharpDX.DirectWrite.Factory FactoryDWrite { get; private set; }

        public RenderTarget DeviceContext { get; private set; }

        private readonly DXCanvas _canvas = new DXCanvas();
        private IDrawable _drawable;
        private Color _backgroundColor;
        private WindowRenderTarget _renderTarget2D;
        private WFGraphicsView _owner;
        private bool _dirty;
        private int _width;
        private int _height;

        public WFGraphicsView GraphicsView
        {
            set
            {
                _owner = value;

                if (Factory2D == null)
                {
                    Factory2D = new Factory();
                    FactoryDWrite = new global::SharpDX.DirectWrite.Factory();
                }

                var properties = new HwndRenderTargetProperties
                {
                    Hwnd = value.Handle,
                    PixelSize = new Size2(value.Width, value.Height),
                    PresentOptions = PresentOptions.None
                };

                _renderTarget2D = new WindowRenderTarget(Factory2D, new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)), properties)
                {
                    AntialiasMode = AntialiasMode.PerPrimitive,
                    TextAntialiasMode = TextAntialiasMode.Cleartype
                };

                DeviceContext = _renderTarget2D.QueryInterface<DeviceContext>();
                _canvas.RenderTarget = DeviceContext;
            }
        }

        public bool Dirty
        {
            get => _dirty;
            set => _dirty = value;
        }

        public ICanvas Canvas => _canvas;

        public IDrawable Drawable
        {
            get => _drawable;
            set => _drawable = value;
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public void Draw(RectangleF dirtyRect)
        {
            try
            {
                _dirty = false;
                StartServiceContext();
                _renderTarget2D.BeginDraw();
                _canvas.SetRenderSize(_width, _height);
                if (_backgroundColor != null)
                {
                    _canvas.FillColor = _backgroundColor;
                    _canvas.FillRectangle(dirtyRect);
                    _canvas.FillColor = Colors.White;
                }

                _drawable.Draw(_canvas, dirtyRect);
            }
            catch (Exception exc)
            {
                Logger.Warn(exc);
            }
            finally
            {
                _renderTarget2D.EndDraw();
                EndServiceContext();
            }
        }

        public void StartServiceContext()
        {
            DXGraphicsService.CurrentFactory.Value = Factory2D;
            DXGraphicsService.CurrentTarget.Value = _canvas.RenderTarget;
        }

        public void EndServiceContext()
        {
            DXGraphicsService.CurrentFactory.Value = null;
            DXGraphicsService.CurrentTarget.Value = null;
        }

        public void SizeChanged(int width, int height)
        {
            _width = width;
            _height = height;
            _renderTarget2D.Resize(new Size2(width, height));
            _owner.Refresh();
        }

        public void Detached()
        {
            // Do nothing
        }

        public void Invalidate()
        {
            _dirty = true;
            _owner.Refresh();
        }

        public void Invalidate(float x, float y, float w, float h)
        {
            _dirty = true;
            _owner.Refresh();
        }

        public void Dispose()
        {
            if (_renderTarget2D != null)
            {
                _renderTarget2D.Dispose();
                _renderTarget2D = null;
                _canvas.RenderTarget = null;
                DeviceContext = null;
            }
        }
    }
}