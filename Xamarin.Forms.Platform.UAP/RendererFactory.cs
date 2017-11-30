using System;

namespace Xamarin.Forms.Platform.UWP
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