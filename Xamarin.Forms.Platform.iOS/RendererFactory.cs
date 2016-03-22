using System;

namespace Xamarin.Forms.Platform.iOS
{
	public static class RendererFactory
	{
		[Obsolete("Use Platform.CreateRenderer")]
		public static IVisualElementRenderer GetRenderer(VisualElement view)
		{
			return Platform.CreateRenderer(view);
		}
	}
}