namespace VisualTestUtils
{
    /// <summary>
    /// IImageEditor allows changing images. It's useful, for instance, for cropping
    /// off the portion of an image that shouldn't be used as part of a visual test.
    /// </summary>
    public interface IImageEditor
    {
        /// <summary>
        /// Get the size of the image, in pixels
        /// </summary>
        /// <returns>image width and height</returns>
        (int width, int height) GetSize();

        /// <summary>
        /// Crop the image, updating it to be the subset in the specified rectangle.
        /// </summary>
        /// <param name="x">crop rectangle left</param>
        /// <param name="y">crop rectangle top</param>
        /// <param name="width">crop rectangle width</param>
        /// <param name="height">crop rectangle height </param>
        void Crop(int x, int y, int width, int height);

        /// <summary>
        /// Create an image snapshot for the updated image, returning it. The snapshot
        /// has the same format (PNG, JPEG, etc.) as the original image.
        /// </summary>
        /// <returns></returns>
        ImageSnapshot GetUpdatedImage();
    }
}
