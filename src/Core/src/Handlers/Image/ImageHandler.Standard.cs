// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();
		public static void MapAspect(IImageHandler handler, IImage image) { }
		public static void MapIsAnimationPlaying(IImageHandler handler, IImage image) { }
		public static void MapSource(IImageHandler handler, IImage image) { }
		void IImageSourcePartSetter.SetImageSource(object? obj) => throw new NotImplementedException();
	}
}