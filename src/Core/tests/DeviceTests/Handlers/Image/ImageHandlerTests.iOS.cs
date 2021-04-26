using System;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageHandlerTests
	{
		UIImageView GetNativeImageView(ImageHandler imageHandler) =>
			(UIImageView)imageHandler.NativeView;

		bool GetNativeIsAnimationPlaying(ImageHandler imageHandler) =>
			GetNativeImageView(imageHandler).IsAnimating;

		Aspect GetNativeAspect(ImageHandler imageHandler) =>
			GetNativeImageView(imageHandler).ContentMode switch
			{
				UIViewContentMode.ScaleAspectFit => Aspect.AspectFit,
				UIViewContentMode.ScaleAspectFill => Aspect.AspectFill,
				UIViewContentMode.ScaleToFill => Aspect.Fill,
				UIViewContentMode.Center => Aspect.Center,
				_ => throw new ArgumentOutOfRangeException("Aspect")
			};
	}
}