using System.Drawing;
using CoreGraphics;
using Foundation;
using UIKit;

namespace System.Graphics.CoreGraphics
{
    [Register("NativeGraphicsView")]
    public class NativeGraphicsView : UIView
    {
        private IGraphicsRenderer _renderer;
        private CGColorSpace _colorSpace;
        private IDrawable _drawable;
        private bool _inPanOrZoom;
        private CGRect _lastBounds;

        public NativeGraphicsView(Drawing.RectangleF frame, IDrawable drawable = null, IGraphicsRenderer renderer = null) : base(frame)
        {
            Drawable = drawable;
            Renderer = renderer;
            BackgroundColor = UIColor.White;
        }

        public NativeGraphicsView(IDrawable drawable = null, IGraphicsRenderer renderer = null)
        {
            Drawable = drawable;
            Renderer = renderer;
            BackgroundColor = UIColor.White;
        }

        public NativeGraphicsView(IntPtr aPtr) : base(aPtr)
        {
            BackgroundColor = UIColor.White;
        }

        public bool InPanOrZoom
        {
            get => _inPanOrZoom;
            set => _inPanOrZoom = value;
        }

        public IGraphicsRenderer Renderer
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

                _renderer = value ?? new DirectRenderer();

                _renderer.GraphicsView = this;
                _renderer.Drawable = _drawable;
                var bounds = Bounds;
                _renderer.SizeChanged((float) bounds.Width, (float) bounds.Height);
            }
        }

        public IDrawable Drawable
        {
            get => _drawable;
            set
            {
                _drawable = value;
                if (_renderer != null)
                {
                    _renderer.Drawable = _drawable;
                    _renderer.Invalidate();
                }
            }
        }

        public void InvalidateDrawable()
        {
            _renderer.Invalidate();
        }

        public void InvalidateDrawable(float x, float y, float w, float h)
        {
            _renderer.Invalidate(x, y, w, h);
        }

        public override void WillMoveToSuperview(UIView newSuperview)
        {
            base.WillMoveToSuperview(newSuperview);

            if (newSuperview == null)
            {
                _renderer.Detached();
            }
        }

        public override void Draw(CGRect dirtyRect)
        {
            base.Draw(dirtyRect);

            if (_drawable == null) return;

            var coreGraphics = UIGraphics.GetCurrentContext();

            if (_colorSpace == null)
            {
                _colorSpace = CGColorSpace.CreateDeviceRGB();
            }

            coreGraphics.SetFillColorSpace(_colorSpace);
            coreGraphics.SetStrokeColorSpace(_colorSpace);
            coreGraphics.SetPatternPhase(PatternPhase);

            _renderer.Draw(coreGraphics, dirtyRect.AsRectangleF(), _inPanOrZoom);
        }

        public override CGRect Bounds
        {
            get => base.Bounds;

            set
            {
                var newBounds = value;
                if (_lastBounds.Width != newBounds.Width || _lastBounds.Height != newBounds.Height)
                {
                    base.Bounds = value;
                    _renderer.SizeChanged((float) newBounds.Width, (float) newBounds.Height);
                    _renderer.Invalidate();

                    _lastBounds = newBounds;
                }
            }
        }

        protected virtual CGSize PatternPhase
        {
            get
            {
                var px = Frame.X;
                var py = Frame.Y;
                return new CGSize(px, py);
            }
        }
    }
}