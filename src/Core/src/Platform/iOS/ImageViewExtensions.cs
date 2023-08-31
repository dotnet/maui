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
			imageView.ClipsToBounds = imageView.ContentMode == UIViewContentMode.ScaleAspectFill;
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
		/// <returns>The size where the image would fit, scaled by its aspect ratio.</returns>
#pragma warning disable RS0016 // Add public types and members to the declared API
		public static CGSize SizeThatFitsImage(this UIImageView imageView, CGSize constraints) // Method name subject to change
#pragma warning restore RS0016 // Add public types and members to the declared API
		{
			var heightConstraint = constraints.Height;
			var widthConstraint = constraints.Width;

			// If there's no image, we don't need to take up any space
			// View Handler will resolve any additional constraints (such as a dimensions being specifically set)
			if (imageView.Image is null)
				return new CGSize(0, 0);

			var imageSize = imageView.Image.Size;
			var imageAspectRatio = imageSize.Width / imageSize.Height;

			// Both Android and Widnows downscale based on width, so we do the same 
			// Large images must be downscaled to fit the container
			// Small images shouldn't be upscaled
			var resultWidth = Math.Min(imageSize.Width, widthConstraint);
			var resultHeight = Math.Min(resultWidth / imageAspectRatio, heightConstraint);

			return new CGSize(resultWidth, resultHeight);
		}
	}
}