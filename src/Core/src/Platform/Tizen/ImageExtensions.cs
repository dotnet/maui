using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Platform
{
	public static class ImageExtensions
	{
		public static void Clear(this Image nativeImage)
		{
		}

		public static void UpdateAspect(this Image nativeImage, IImage image)
		{
			nativeImage.Aspect = image.Aspect.ToNative();
		}

		public static void UpdateIsAnimationPlaying(this Image nativeImage, IImageSourcePart image)
		{
			nativeImage.IsAnimated = image.IsAnimationPlaying;
			nativeImage.IsAnimationPlaying = image.IsAnimationPlaying;
		}
	}
}
