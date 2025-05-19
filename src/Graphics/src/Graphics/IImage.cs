using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Specifies how an image should be resized to fit a target area.
	/// </summary>
	public enum ResizeMode
	{
		/// <summary>
		/// Preserves aspect ratio and ensures the image fits within the target dimensions.
		/// May leave empty space if the aspect ratios don't match.
		/// </summary>
		Fit,

		/// <summary>
		/// Preserves aspect ratio and fills the target dimensions, potentially cropping the image.
		/// </summary>
		Bleed,

		/// <summary>
		/// Ignores aspect ratio and stretches the image to exactly fit the target dimensions.
		/// </summary>
		Stretch
	}

	/// <summary>
	/// Defines an interface for an image that can be drawn onto a canvas and manipulated.
	/// </summary>
	public interface IImage : IDrawable, IDisposable
	{
		/// <summary>
		/// Gets the width of the image in pixels.
		/// </summary>
		float Width { get; }

		/// <summary>
		/// Gets the height of the image in pixels.
		/// </summary>
		float Height { get; }

		/// <summary>
		/// Creates a downsized version of the image with the same aspect ratio.
		/// </summary>
		/// <param name="maxWidthOrHeight">The maximum width or height in pixels.</param>
		/// <param name="disposeOriginal">Whether to dispose the original image after downsizing.</param>
		/// <returns>A new <see cref="IImage"/> with the downsized dimensions.</returns>
		IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false);

		/// <summary>
		/// Creates a downsized version of the image that fits within the specified dimensions.
		/// </summary>
		/// <param name="maxWidth">The maximum width in pixels.</param>
		/// <param name="maxHeight">The maximum height in pixels.</param>
		/// <param name="disposeOriginal">Whether to dispose the original image after downsizing.</param>
		/// <returns>A new <see cref="IImage"/> with the downsized dimensions.</returns>
		IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false);

		/// <summary>
		/// Resizes the image to the specified dimensions using the specified resize mode.
		/// </summary>
		/// <param name="width">The target width in pixels.</param>
		/// <param name="height">The target height in pixels.</param>
		/// <param name="resizeMode">The resize mode to use (default is Fit).</param>
		/// <param name="disposeOriginal">Whether to dispose the original image after resizing.</param>
		/// <returns>A new <see cref="IImage"/> with the resized dimensions.</returns>
		IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false);

		/// <summary>
		/// Saves the image to a stream in the specified format.
		/// </summary>
		/// <param name="stream">The stream to save the image to.</param>
		/// <param name="format">The format to save the image in (default is PNG).</param>
		/// <param name="quality">The quality level (0.0 to 1.0) when using lossy formats like JPEG (default is 1.0).</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="quality"/> is not between 0.0 and 1.0.</exception>
		void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1);

		/// <summary>
		/// Asynchronously saves the image to a stream in the specified format.
		/// </summary>
		/// <param name="stream">The stream to save the image to.</param>
		/// <param name="format">The format to save the image in (default is PNG).</param>
		/// <param name="quality">The quality level (0.0 to 1.0) when using lossy formats like JPEG (default is 1.0).</param>
		/// <returns>A task representing the asynchronous save operation.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="quality"/> is not between 0.0 and 1.0.</exception>
		Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1);

		/// <summary>
		/// Creates a platform-specific representation of this image.
		/// </summary>
		/// <returns>A platform-specific <see cref="IImage"/> representation.</returns>
		IImage ToPlatformImage();
	}
}
