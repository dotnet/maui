using System;
using ImageMagick;

namespace VisualTestUtils.MagickNet
{
    public class MagickNetImageEditor : IImageEditor
    {
        MagickImage _magickImage;

        public MagickNetImageEditor(ImageSnapshot imageSnapshot)
        {
            _magickImage = new MagickImage(imageSnapshot.Data);
        }

        public void Crop(int x, int y, int width, int height)
        {
            _magickImage.Crop(new MagickGeometry(x, y, width, height));
            _magickImage.RePage();
        }

        public (int width, int height) GetSize() =>
            (_magickImage.Width, _magickImage.Height);

        public ImageSnapshot GetUpdatedImage()
        {
            ImageSnapshotFormat format = _magickImage.Format switch
            {
                MagickFormat.Png => ImageSnapshotFormat.PNG,
                MagickFormat.Jpeg => ImageSnapshotFormat.JPEG,
                _ => throw new NotSupportedException($"Unexpected image format: {_magickImage.Format}")
            };

            return new ImageSnapshot(_magickImage.ToByteArray(), format);
        }
    }
}
