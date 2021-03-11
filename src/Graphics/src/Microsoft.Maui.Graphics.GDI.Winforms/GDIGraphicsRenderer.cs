namespace Microsoft.Maui.Graphics.GDI
{
    public interface GDIGraphicsRenderer : IDisposable
    {
        GDIGraphicsView GraphicsView { set; }

        ICanvas Canvas { get; }

        Color BackgroundColor { get; set; }

        IDrawable Drawable { get; set; }

        void Draw(System.Drawing.Graphics graphics, RectangleF dirtyRect);

        void SizeChanged(int width, int height);

        void Detached();

        bool Dirty { get; set; }

        void Invalidate();

        void Invalidate(float x, float y, float w, float h);
    }
}