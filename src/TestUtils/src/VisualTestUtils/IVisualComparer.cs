namespace VisualTestUtils
{
    /// <summary>
    /// Interface for image visual comparison.
    /// </summary>
    public interface IVisualComparer
    {
        /// <summary>
        /// Compare the image against the provided baseline, returning the percentage difference (0.01 = 1% difference).
        /// </summary>
        /// <param name="baselineImage">Baseline Image Bytes.</param>
        /// <param name="actualImage">Actual Image Bytes.</param>
        /// <returns>Percentage difference.</returns>
        ImageDifference Compare(ImageSnapshot baselineImage, ImageSnapshot actualImage);
    }
}
