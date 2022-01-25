#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation.Metadata;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WImageSource = Microsoft.UI.Xaml.Media.ImageSource;

namespace Microsoft.Maui.Platform
{
	public static class ImageViewExtensions
	{
		const string BitmapImageTypeName = "Microsoft.UI.Xaml.Media.Imaging.BitmapImage";

		static bool IsAnimationSupported;

		static ImageViewExtensions()
		{
			IsAnimationSupported =
				ApiInformation.IsPropertyPresent(BitmapImageTypeName, nameof(BitmapImage.IsAnimatedBitmap)) &&
				ApiInformation.IsPropertyPresent(BitmapImageTypeName, nameof(BitmapImage.IsPlaying)) &&
				ApiInformation.IsPropertyPresent(BitmapImageTypeName, nameof(BitmapImage.Play)) &&
				ApiInformation.IsPropertyPresent(BitmapImageTypeName, nameof(BitmapImage.Stop));
		}

		public static void Clear(this WImage imageView)
		{
			imageView.Source = null;
		}

		public static void UpdateAspect(this WImage imageView, IImage image)
		{
			imageView.Stretch = image.Aspect.ToStretch();
		}

		public static void UpdateIsAnimationPlaying(this WImage imageView, IImageSourcePart image)
		{
			if (!IsAnimationSupported)
				return;

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