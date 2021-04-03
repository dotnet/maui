namespace Microsoft.Maui.Graphics
{
    public static class PictureExtensions
    {
        public static RectangleF GetBounds(this IPicture target)
        {
            if (target == null) return default;
            return new RectangleF(target.X, target.Y, target.Width, target.Height);
        }
    }
}
