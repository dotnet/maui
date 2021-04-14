using UIKit;

namespace Microsoft.Maui
{
	public static class ScrollViewExtensions
	{
		public static void UpdateContent(this UIScrollView nativeScrollView, IScroll scrollView, IMauiContext? mauiContext)
		{
			IView content = scrollView.Content;

			if (content == null || mauiContext == null)
				return;

			UIView nativeView = content.ToNative(mauiContext);

			if (nativeView.Superview != null)
				nativeView.RemoveFromSuperview();

			nativeScrollView.AddSubview(nativeView);
		}
	}
}