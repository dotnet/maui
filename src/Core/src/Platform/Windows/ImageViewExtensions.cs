#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui.Platform
{
	public static class ImageViewExtensions
	{
		public static void Clear(this WImage imageView)
		{
			imageView.Source = null;
		}

		public static void UpdateAspect(this WImage imageView, IImage image)
		{
			imageView.Stretch = image.Aspect.ToStretch();

			// For AspectFill, we still want center alignment on the image itself
			// but for other aspects, the layout alignment should be controlled by
			// the container or the layout alignment properties
			if (image.Aspect == Aspect.AspectFill)
			{
				imageView.VerticalAlignment = VerticalAlignment.Center;
				imageView.HorizontalAlignment = HorizontalAlignment.Center;
			}
			else
			{
				// Clear any previous alignment settings when not AspectFill
				// The layout alignment will be handled by the container or layout alignment properties
				imageView.ClearValue(FrameworkElement.VerticalAlignmentProperty);
				imageView.ClearValue(FrameworkElement.HorizontalAlignmentProperty);
			}
		}

		public static void UpdateHorizontalLayoutAlignment(this FrameworkElement platformView, IView view)
		{
			platformView.HorizontalAlignment = view.HorizontalLayoutAlignment switch
			{
				Primitives.LayoutAlignment.Start => HorizontalAlignment.Left,
				Primitives.LayoutAlignment.Center => HorizontalAlignment.Center,
				Primitives.LayoutAlignment.End => HorizontalAlignment.Right,
				Primitives.LayoutAlignment.Fill => HorizontalAlignment.Stretch,
				_ => HorizontalAlignment.Stretch
			};
		}

		public static void UpdateVerticalLayoutAlignment(this FrameworkElement platformView, IView view)
		{
			platformView.VerticalAlignment = view.VerticalLayoutAlignment switch
			{
				Primitives.LayoutAlignment.Start => VerticalAlignment.Top,
				Primitives.LayoutAlignment.Center => VerticalAlignment.Center,
				Primitives.LayoutAlignment.End => VerticalAlignment.Bottom,
				Primitives.LayoutAlignment.Fill => VerticalAlignment.Stretch,
				_ => VerticalAlignment.Stretch
			};
		}

		public static void UpdateIsAnimationPlaying(this WImage imageView, IImageSourcePart image)
		{
			if (imageView.Source is BitmapImage bitmapImage && bitmapImage.IsAnimatedBitmap)
			{
				if (image.IsAnimationPlaying)
				{
					if (!bitmapImage.IsPlaying)
						bitmapImage.Play();
				}
				else
				{
					if (bitmapImage.IsPlaying)
						bitmapImage.Stop();
				}
			}
		}
	}
}