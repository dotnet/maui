#nullable enable
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui
{
	/// <summary>
	/// Describes the size an image is displayed at, so that large images can be decoded at that size instead of their full size.
	/// </summary>
	sealed class ImageDecodeOptions
	{
		public ImageDecodeOptions(double pixelWidth, double pixelHeight, Aspect aspect, bool isViewSizeKnown)
		{
			PixelWidth = pixelWidth;
			PixelHeight = pixelHeight;
			Aspect = aspect;
			IsViewSizeKnown = isViewSizeKnown;
		}

		/// <summary>The width the image is displayed at, in pixels.</summary>
		public double PixelWidth { get; }

		/// <summary>The height the image is displayed at, in pixels.</summary>
		public double PixelHeight { get; }

		public Aspect Aspect { get; }

		/// <summary>
		/// Whether the size is the size of the view (or its requested size), rather than an estimate such as the window size.
		/// </summary>
		public bool IsViewSizeKnown { get; }

		/// <summary>Set by the service when it decoded the image at a smaller size than its full size.</summary>
		public bool WasDownsampled { get; set; }

		/// <summary>
		/// Whether an image decoded for these options is not sharp enough for <paramref name="required"/>.
		/// </summary>
		public bool IsInsufficientFor(ImageDecodeOptions required)
		{
			// Small changes do not justify decoding the image again.
			const double Threshold = 1.25;

			return required.Aspect != Aspect ||
				required.PixelWidth > PixelWidth * Threshold ||
				required.PixelHeight > PixelHeight * Threshold;
		}
	}

	/// <summary>
	/// An image source service that can decode images at the size they are displayed at.
	/// </summary>
	interface IImageSourceServiceWithDecodeOptions
	{
		Task<IImageSourceServiceResult<UIImage>?> GetImageAsync(IImageSource imageSource, float scale, ImageDecodeOptions options, CancellationToken cancellationToken);
	}
}
