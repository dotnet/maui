using System.Collections.Generic;
#if __UNIFIED__
using CoreGraphics;
using UIKit;

#else
using MonoTouch.UIKit;
#endif

#if !__UNIFIED__
	// Save ourselves a ton of ugly ifdefs below
using CGSize = System.Drawing.SizeF;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public delegate SizeRequest? GetDesiredSizeDelegate(NativeViewWrapperRenderer renderer, double widthConstraint, double heightConstraint);

	public delegate CGSize? SizeThatFitsDelegate(CGSize size);

	public delegate bool LayoutSubviewsDelegate();

	public static class LayoutExtensions
	{
		public static void Add(this IList<View> children, UIView view, GetDesiredSizeDelegate getDesiredSizeDelegate = null, SizeThatFitsDelegate sizeThatFitsDelegate = null,
							   LayoutSubviewsDelegate layoutSubViews = null)
		{
			children.Add(view.ToView(getDesiredSizeDelegate, sizeThatFitsDelegate, layoutSubViews));
		}

		public static View ToView(this UIView view, GetDesiredSizeDelegate getDesiredSizeDelegate = null, SizeThatFitsDelegate sizeThatFitsDelegate = null, LayoutSubviewsDelegate layoutSubViews = null)
		{
			return new NativeViewWrapper(view, getDesiredSizeDelegate, sizeThatFitsDelegate, layoutSubViews);
		}
	}
}