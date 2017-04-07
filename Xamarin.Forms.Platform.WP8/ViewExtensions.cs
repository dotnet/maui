using System;

namespace Xamarin.Forms.Platform.WinPhone
{
	public static class ViewExtensions
	{
		[Obsolete("GetRenderer is obsolete as of version 2.0.1. Please use Platform.GetRenderer instead.")]
		public static IVisualElementRenderer GetRenderer(this VisualElement self)
		{
			return Platform.GetRenderer(self);
		}

		[Obsolete("SetRenderer is obsolete as of version 2.0.1. Please use Platform.SetRenderer instead.")]
		public static void SetRenderer(this VisualElement self, IVisualElementRenderer renderer)
		{
			Platform.SetRenderer(self, renderer);
		}
	}
}