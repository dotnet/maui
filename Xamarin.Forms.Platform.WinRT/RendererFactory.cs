using System;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public static class RendererFactory
	{
		[Obsolete("GetRenderer is obsolete as of version 2.0.1. Please use Platform.CreateRenderer instead.")]
		public static IVisualElementRenderer CreateRenderer(VisualElement element)
		{
			return Platform.CreateRenderer(element);
		}
	}
}