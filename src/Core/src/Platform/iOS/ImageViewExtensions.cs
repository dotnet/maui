
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
			// stop the animation if there is one
			imageView.StopAnimating();
			imageView.AnimationImages = null;
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

		// TODO: This method does not appear to be used, should we obsolete in net9?
		public static void UpdateSource(this UIImageView imageView, UIImage? uIImage, IImageSourcePart image)
		{
			imageView.Image = uIImage;
			imageView.UpdateIsAnimationPlaying(image);
		}

		// TODO: This method does not appear to be used, should we obsolete in net9?
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
		/// <param name="padding"></param>
		/// <param name="widthConstraintIsExplicit">Whether the width constraint comes from an explicit request (e.g. WidthRequest) and may therefore scale the image beyond its native width.</param>
		/// <param name="heightConstraintIsExplicit">Whether the height constraint comes from an explicit request (e.g. HeightRequest) and may therefore scale the image beyond its native height.</param>
		/// <returns>The size where the image would fit depending on the aspect ratio.</returns>
		internal static CGSize SizeThatFitsImage(
			this UIImageView imageView,
			CGSize constraints,
			Thickness padding = default,
			bool widthConstraintIsExplicit = false,
			bool heightConstraintIsExplicit = false)
		{
			// If there's no image, we don't need to take up any space
			if (imageView.Image is null)
			{
				return new CGSize(0, 0);
			}

			CGSize imageSize = imageView.Image.Size;
			double imageWidth = imageSize.Width;
			double imageHeight = imageSize.Height;

			var horizontalThickness = padding.HorizontalThickness;
			var verticalThickness = padding.VerticalThickness;

			double widthConstraint = constraints.Width - horizontalThickness;
			double heightConstraint = constraints.Height - verticalThickness;

			var constrainedWidth = Math.Min(imageWidth, widthConstraint);
			var constrainedHeight = Math.Min(imageHeight, heightConstraint);

			// In cases where we the image must fit its given constraints, we must shrink based on the smallest dimension (scale factor)
			// that can fit it
			if (imageView.ContentMode == UIViewContentMode.ScaleAspectFit)
			{
				// Compute the raw (uncapped) ratio for each axis. When an axis constraint is +Infinity
				// (i.e. unconstrained, as happens on the cross axis of a StackLayout), its ratio is also
				// +Infinity, meaning it never limits the scale factor - only the other (finite/explicit) axis does.
				var widthRatio =
					double.IsPositiveInfinity(widthConstraint)
						? double.PositiveInfinity
						: widthConstraint / imageWidth;

				var heightRatio =
					double.IsPositiveInfinity(heightConstraint)
						? double.PositiveInfinity
						: heightConstraint / imageHeight;

				var scaleFactor = Math.Min(widthRatio, heightRatio);

				// Only when NEITHER axis constraint came from an explicit request (WidthRequest/HeightRequest) -
				// i.e. we're simply fitting within available space, not honoring an explicit enlarge request -
				// do we cap the scale factor at 1 so the image never grows beyond its native size.
				if (!widthConstraintIsExplicit && !heightConstraintIsExplicit)
				{
					scaleFactor = Math.Min(scaleFactor, 1);
				}

				return new CGSize(imageWidth * scaleFactor + horizontalThickness, imageHeight * scaleFactor + verticalThickness);
			}

			// Cases where AspectMode is ScaleToFill or Center
			return new CGSize(constrainedWidth + horizontalThickness, constrainedHeight + verticalThickness);
		}
	}
}