using System;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public static class RendererFactory
	{
		[Obsolete("Use Platform.CreateRenderer")]
		public static IVisualElementRenderer CreateRenderer(VisualElement element)
		{
			return Platform.CreateRenderer(element);
		}
	}
}