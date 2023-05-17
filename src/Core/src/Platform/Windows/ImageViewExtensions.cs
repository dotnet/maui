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

		public static void UpdateAspect(this WImage imageView, IImage image, WrapperView wrapper)
		{
			imageView.Stretch = image.Aspect.ToStretch();
			imageView.VerticalAlignment = VerticalAlignment.Center;
			imageView.HorizontalAlignment = HorizontalAlignment.Center;
			wrapper.Height = image.Height;
			wrapper.Width = image.Width;
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