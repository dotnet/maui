using System;

namespace Microsoft.Maui
{
	public interface IAdornerService
	{
		void HandlePoint(Action<Point> onTouchDown);
		IView? GetViewAtPoint(ILayout layout, Point point);
		IView? GetViewAtPoint(IWindow window, Point point);

		void ClearAdorners();
#if __ANDROID__
		Android.Views.ViewOverlay? GetAdornerLayer();
#elif __IOS__
		CoreAnimation.CALayer? GetAdornerLayer();
#endif
	}
}
