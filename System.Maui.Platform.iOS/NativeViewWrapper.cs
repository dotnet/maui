#if __MOBILE__
using UIKit;
namespace Xamarin.Forms.Platform.iOS
#else
using UIView = AppKit.NSView;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	public class NativeViewWrapper : View
	{
		public NativeViewWrapper(UIView nativeView, GetDesiredSizeDelegate getDesiredSizeDelegate = null,
			SizeThatFitsDelegate sizeThatFitsDelegate = null, LayoutSubviewsDelegate layoutSubViews = null)
		{
			GetDesiredSizeDelegate = getDesiredSizeDelegate;
			SizeThatFitsDelegate = sizeThatFitsDelegate;
			LayoutSubViews = layoutSubViews;
			NativeView = nativeView;

			nativeView.TransferbindablePropertiesToWrapper(this);
		}

		public GetDesiredSizeDelegate GetDesiredSizeDelegate { get; }

		public LayoutSubviewsDelegate LayoutSubViews { get; set; }

		public UIView NativeView { get; }

		public SizeThatFitsDelegate SizeThatFitsDelegate { get; set; }

		protected override void OnBindingContextChanged()
		{
			NativeView.SetBindingContext(BindingContext, nv => nv.Subviews);
			base.OnBindingContextChanged();
		}
	}
}