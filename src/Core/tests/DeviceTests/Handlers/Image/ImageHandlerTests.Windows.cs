using System;
using Microsoft.UI.Xaml.Media.Imaging;
using WImage = Microsoft.UI.Xaml.Controls.Image;
using WStretch = Microsoft.UI.Xaml.Media.Stretch;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageHandlerTests<TImageHandler, TStub>
	{
		WImage GetPlatformImageView(IImageHandler imageHandler) =>
			imageHandler.PlatformView;

		bool GetNativeIsAnimationPlaying(IImageHandler imageHandler) =>
			GetPlatformImageView(imageHandler).Source is BitmapImage bitmapImage && bitmapImage.IsPlaying;

		Aspect GetNativeAspect(IImageHandler imageHandler) =>
			GetPlatformImageView(imageHandler).Stretch switch
			{
				WStretch.Uniform => Aspect.AspectFit,
				WStretch.UniformToFill => Aspect.AspectFill,
				WStretch.Fill => Aspect.Fill,
				WStretch.None => Aspect.Center,
				_ => throw new ArgumentOutOfRangeException("Stretch")
			};
	}
}