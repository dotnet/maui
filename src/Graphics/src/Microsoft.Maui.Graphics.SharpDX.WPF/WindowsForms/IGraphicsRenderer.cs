namespace Microsoft.Maui.Graphics.SharpDX.WindowsForms
{
    public interface IGraphicsRenderer : IDisposable
    {
        WFGraphicsView GraphicsView { set; }

        ICanvas Canvas { get; }

        Color BackgroundColor { get; set; }

        IDrawable Drawable { get; set; }

        void Draw(RectangleF dirtyRect);

        void SizeChanged(int width, int height);

        void Detached();

        bool Dirty { get; set; }

        void Invalidate();

        void Invalidate(float x, float y, float w, float h);

        void StartServiceContext();

        void EndServiceContext();
    }
}