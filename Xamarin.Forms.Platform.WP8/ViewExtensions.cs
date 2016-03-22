using System;

namespace Xamarin.Forms.Platform.WinPhone
{
	public static class ViewExtensions
	{
		[Obsolete("Use Platform.GetRenderer")]
		public static IVisualElementRenderer GetRenderer(this VisualElement self)
		{
			return Platform.GetRenderer(self);
		}

		[Obsolete("Use Platform.SetRenderer")]
		public static void SetRenderer(this VisualElement self, IVisualElementRenderer renderer)
		{
			Platform.SetRenderer(self, renderer);
		}
	}
}