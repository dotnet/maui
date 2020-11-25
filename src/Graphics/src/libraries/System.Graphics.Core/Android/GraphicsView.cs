using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace System.Graphics.Android
{
    public class GraphicsView : View
    {
        private RectangleF _dirtyRect;
        private IGraphicsRenderer _renderer;
        private IDrawable _drawable;
        private int _width, _height;
        private bool _inPanOrZoom;

        public GraphicsView(Context context, IAttributeSet attrs, IDrawable drawable = null, IGraphicsRenderer renderer = null) : base(context, attrs)
        {
            Drawable = drawable;
            Renderer = renderer;
        }

        public GraphicsView(Context context, IDrawable drawable = null, IGraphicsRenderer renderer = null) : base(context)
        {
            Drawable = drawable;
            Renderer = renderer;
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

                _renderer = value ?? new DirectRenderer(Context);

                _renderer.GraphicsView = this;
                _renderer.Drawable = _drawable;
                _renderer.SizeChanged(_width, _height);
            }
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
                if (_renderer != null)
                {
                    _renderer.Drawable = _drawable;
                }
            }
        }

        public override void Draw(Canvas androidCanvas)
        {
            if (_drawable == null) return;

            _dirtyRect.Width = Width;
            _dirtyRect.Height = Height;
            _renderer.Draw(androidCanvas, _dirtyRect, _inPanOrZoom);
        }

        protected override void OnSizeChanged(int width, int height, int oldWidth, int oldHeight)
        {
            base.OnSizeChanged(width, height, oldWidth, oldHeight);
            _renderer.SizeChanged(width, height);
            _width = width;
            _height = height;
        }

        protected override void OnDetachedFromWindow()
        {
            _renderer.Detached();
        }

        public void InvalidateDrawable()
        {
            _renderer.Invalidate();
        }

        public void InvalidateDrawable(float x, float y, float w, float h)
        {
            _renderer.Invalidate(x, y, w, h);
        }
    }
}