// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ImageHandlerTests<TImageHandler, TStub>
	{
		UIImageView GetPlatformImageView(IImageHandler imageHandler) =>
			imageHandler.PlatformView;

		bool GetNativeIsAnimationPlaying(IImageHandler imageHandler) =>
			GetPlatformImageView(imageHandler).IsAnimating;

		Aspect GetNativeAspect(IImageHandler imageHandler) =>
			GetPlatformImageView(imageHandler).ContentMode switch
			{
				UIViewContentMode.ScaleAspectFit => Aspect.AspectFit,
				UIViewContentMode.ScaleAspectFill => Aspect.AspectFill,
				UIViewContentMode.ScaleToFill => Aspect.Fill,
				UIViewContentMode.Center => Aspect.Center,
				_ => throw new ArgumentOutOfRangeException("Aspect")
			};
	}
}