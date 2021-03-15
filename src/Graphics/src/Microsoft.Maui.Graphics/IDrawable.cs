namespace Microsoft.Maui.Graphics
{
    public interface IDrawable
    {
        void Draw(ICanvas canvas, RectangleF dirtyRect);
    }
}