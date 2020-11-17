using Android.Graphics;

namespace System.Graphics.Android
{
    public interface IGraphicsRenderer : IDisposable
    {
        GraphicsView GraphicsView { set; }
        ICanvas Canvas { get; }
        IDrawable Drawable { get; set; }
        Color BackgroundColor { get; set; }
        void Draw(Canvas androidCanvas, RectangleF dirtyRect, bool inPanOrZoom);
        void SizeChanged(int width, int height);
        void Detached();
        void Invalidate();
        void Invalidate(float x, float y, float w, float h);
    }
}