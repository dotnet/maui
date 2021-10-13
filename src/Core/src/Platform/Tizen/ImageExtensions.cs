using System;
using System.Threading;
using System.Threading.Tasks;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Platform
{
	public static class ImageExtensions
	{
		public static void Clear(this Image platformImage)
		{
		}

		public static void UpdateAspect(this Image platformImage, IImage image)
		{
			platformImage.Aspect = image.Aspect.ToPlatform();
		}

		public static void UpdateIsAnimationPlaying(this Image platformImage, IImageSourcePart image)
		{
			platformImage.SetIsAnimationPlaying(image.IsAnimationPlaying);
		}
	}
}
