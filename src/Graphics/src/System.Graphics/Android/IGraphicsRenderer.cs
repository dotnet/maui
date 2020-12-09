using Android.Graphics;

namespace System.Graphics.Android
{
    public interface IGraphicsRenderer : IDisposable
    {
        NativeGraphicsView GraphicsView { set; }
        ICanvas Canvas { get; }
        IDrawable Drawable { get; set; }
        Color BackgroundColor { get; set; }
        void Draw(Canvas androidCanvas, RectangleF dirtyRect);
        void SizeChanged(int width, int height);
        void Detached();
        void Invalidate();
        void Invalidate(float x, float y, float w, float h);
    }
}