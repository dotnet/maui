using System;

namespace Xamarin.Forms.Platform.Android
{
	public static class VisualElementExtensions
	{
		public static IVisualElementRenderer GetRenderer(this VisualElement self)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			IVisualElementRenderer renderer = Platform.GetRenderer(self);

			return renderer;
		}
	}
}