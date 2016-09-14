using System.Collections.Generic;
using CoreGraphics;
using UIKit;

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