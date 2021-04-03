namespace Microsoft.Maui.Graphics
{
    public interface IPicture
    {
        void Draw(ICanvas canvas);

        float X { get; }

        float Y { get; }

        float Width { get; }

        float Height { get; }
    }
}
