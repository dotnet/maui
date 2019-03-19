using System;
using CoreAnimation;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	public static class ImageExtensions
	{
		public static NSString ToNSViewContentMode(this Aspect aspect)
		{
			switch (aspect)
			{
				case Aspect.AspectFill:
					return CALayer.GravityResizeAspectFill;
				case Aspect.Fill:
					return CALayer.GravityResize;
				case Aspect.AspectFit:
				default:
					return CALayer.GravityResizeAspect;
			}
		}
	}
}
