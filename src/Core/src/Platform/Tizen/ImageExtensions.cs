using Tizen.UIExtensions.ElmSharp;

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
			platformImage.IsAnimated = image.IsAnimationPlaying;
			platformImage.IsAnimationPlaying = image.IsAnimationPlaying;
		}
	}
}
