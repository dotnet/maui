using System.Collections.Generic;
using CoreGraphics;
#if __MOBILE__
using UIKit;
using Xamarin.Forms.Platform.iOS;
namespace Xamarin.Forms.Platform.iOS
#else
using AppKit;
using UIView = AppKit.NSView;
using Xamarin.Forms.Platform.MacOS;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	public delegate SizeRequest? GetDesiredSizeDelegate(
		NativeViewWrapperRenderer renderer, double widthConstraint, double heightConstraint);

	public delegate CGSize? SizeThatFitsDelegate(CGSize size);

	public delegate bool LayoutSubviewsDelegate();

	public static class LayoutExtensions
	{
		public static void Add(this IList<View> children, UIView view, GetDesiredSizeDelegate getDesiredSizeDelegate = null,
			SizeThatFitsDelegate sizeThatFitsDelegate = null,
			LayoutSubviewsDelegate layoutSubViews = null)
		{
			children.Add(view.ToView(getDesiredSizeDelegate, sizeThatFitsDelegate, layoutSubViews));
		}

		public static View ToView(this UIView view, GetDesiredSizeDelegate getDesiredSizeDelegate = null,
			SizeThatFitsDelegate sizeThatFitsDelegate = null, LayoutSubviewsDelegate layoutSubViews = null)
		{
			return new NativeViewWrapper(view, getDesiredSizeDelegate, sizeThatFitsDelegate, layoutSubViews);
		}
	}
}