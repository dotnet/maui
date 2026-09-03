 using ImageMagick;

namespace VisualTestUtils.MagickNet
{
  
    public class MagickNetVisualComparer : IVisualComparer
    {
        private ErrorMetric _errorMetric;
        private double _differenceThreshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="MagickNetVisualComparer"/> class.
        /// </summary>
        /// <param name="errorMetric">The error metric to use.</param>
        /// <param name="differenceThreshold">The difference threshold.</param>
        public MagickNetVisualComparer(ErrorMetric errorMetric = ErrorMetric.RootMeanSquared, double differenceThreshold = 0.005)
        {
            _errorMetric = errorMetric;
            _differenceThreshold = differenceThreshold;
        }

        /// <inheritdoc />
        public ImageDifference? Compare(ImageSnapshot baselineImage, ImageSnapshot actualImage)
        {
            using var magickBaselineImage = new MagickImage(baselineImage.Data);
            using var magickActualImage = new MagickImage(actualImage.Data);

            ImageSizeDifference? imageSizeDifference = ImageSizeDifference.Compare((int)magickBaselineImage.Width, (int)magickBaselineImage.Height, (int)magickActualImage.Width, (int)magickActualImage.Height);
            if (imageSizeDifference != null)
                return imageSizeDifference;

            double distortionDifference = magickBaselineImage.Compare(magickActualImage, _errorMetric, Channels.Red);
            if (distortionDifference > this._differenceThreshold)
                return new ImagePercentageDifference(distortionDifference);

            return null;
        }
    }
}
