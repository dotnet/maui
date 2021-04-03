using System;
using System.Windows.Forms;
using SharpDX.Desktop;

namespace Microsoft.Maui.Graphics.SharpDX.WindowsForms
{
    public class WFGraphicsView : RenderControl
    {
        private IGraphicsRenderer _renderer;
        private IDrawable _drawable;

        public WFGraphicsView(IDrawable drawable = null, IGraphicsRenderer renderer = null)
        {
            Drawable = drawable;
            Renderer = renderer;
        }

        public bool Dirty
        {
            get => _renderer.Dirty;
            set => _renderer.Dirty = value;
        }

        public Color BackgroundColor
        {
            get => _renderer.BackgroundColor;
            set => _renderer.BackgroundColor = value;
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

                _renderer = value ?? new WFDirectGraphicsRenderer();

                _renderer.GraphicsView = this;
                _renderer.Drawable = _drawable;
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
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_drawable == null) return;

            var clipRect = e.ClipRectangle;
            _renderer.Draw(new RectangleF(clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height));
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _renderer.SizeChanged(Width, Height);
        }
    }
}
