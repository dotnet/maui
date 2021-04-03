namespace Microsoft.Maui.Graphics
{
    public class PicturePattern : AbstractPattern
    {
        private readonly IPicture _picture;

        public PicturePattern(IPicture picture, float stepX, float stepY) : base(picture.Width, picture.Height, stepX, stepY)
        {
            _picture = picture;
        }

        public PicturePattern(IPicture picture) : base(picture.Width, picture.Height)
        {
            _picture = picture;
        }

        public override void Draw(ICanvas canvas)
        {
            _picture.Draw(canvas);
        }
    }
}
