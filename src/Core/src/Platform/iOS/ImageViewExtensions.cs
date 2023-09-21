#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ImageViewExtensions
	{
		public static void Clear(this UIImageView imageView)
		{
			imageView.Image = null;
		}

		public static void UpdateAspect(this UIImageView imageView, IImage image)
		{
			imageView.ContentMode = image.Aspect.ToUIViewContentMode();
			imageView.ClipsToBounds = imageView.ContentMode == UIViewContentMode.ScaleAspectFill || imageView.ContentMode == UIViewContentMode.Center;
		}

		public static void UpdateIsAnimationPlaying(this UIImageView imageView, IImageSourcePart image)
		{
			if (image.IsAnimationPlaying)
			{
				if (!imageView.IsAnimating)
					imageView.StartAnimating();
			}
			else
			{
				if (imageView.IsAnimating)
					imageView.StopAnimating();
			}
		}

		public static void UpdateSource(this UIImageView imageView, UIImage? uIImage, IImageSourcePart image)
		{
			imageView.Image = uIImage;
			imageView.UpdateIsAnimationPlaying(image);
		}

		public static Task<IImageSourceServiceResult<UIImage>?> UpdateSourceAsync(
			this UIImageView imageView,
			IImageSourcePart image,
			IImageSourceServiceProvider services,
			CancellationToken cancellationToken = default)
		{
			float scale = imageView.Window?.GetDisplayDensity() ?? 1.0f;

			imageView.Clear();
			return image.UpdateSourceAsync(imageView, services, (uiImage) =>
			{
				imageView.Image = uiImage;
			}, scale, cancellationToken);
		}

		/// <summary>
		/// Gets the size that fits on the screen for a <see cref="UIImageView"/> to be consistent cross-platform.
		/// </summary>
		/// <remarks>The default iOS implementation of SizeThatFits only returns the image's dimensions and ignores the constraints.</remarks>
		/// <param name="imageView">The <see cref="UIImageView"/> to be measured.</param>
		/// <param name="constraints">The specified size constraints.</param>
		/// <returns>The size where the image would fit depending on the aspect ratio.</returns>
		internal static CGSize SizeThatFitsImage(this UIImageView imageView, CGSize constraints)
		{
			// If there's no image, we don't need to take up any space
			if (imageView.Image is null)
				return new CGSize(0, 0);

			var heightConstraint = constraints.Height;
			var widthConstraint = constraints.Width;
			var imageSize = imageView.Image.Size;

			var widthRatio = Math.Min(imageSize.Width, widthConstraint) / imageSize.Width;
			var heightRatio = Math.Min(imageSize.Height, heightConstraint) / imageSize.Height;

			// In cases where we the image must fit its given constraints, we must shrink based on the smallest dimension (scale factor)
			// that can fit it
			if(imageView.ContentMode == UIViewContentMode.ScaleAspectFit)
			{
				var scaleFactor = Math.Min(widthRatio, heightRatio);
				return new CGSize(imageSize.Width * scaleFactor, imageSize.Height * scaleFactor);
			}

			// Cases where AspectMode is ScaleToFill or Center
			return constraints;
		}
	}
}