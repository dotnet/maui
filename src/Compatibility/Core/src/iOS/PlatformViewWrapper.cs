#if __MOBILE__
using ObjCRuntime;
using UIKit;
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using UIView = AppKit.NSView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public class PlatformViewWrapper : View
	{
		public PlatformViewWrapper(UIView nativeView, GetDesiredSizeDelegate getDesiredSizeDelegate = null,
			SizeThatFitsDelegate sizeThatFitsDelegate = null, LayoutSubviewsDelegate layoutSubViews = null)
		{
			GetDesiredSizeDelegate = getDesiredSizeDelegate;
			SizeThatFitsDelegate = sizeThatFitsDelegate;
			LayoutSubViews = layoutSubViews;
			PlatformView = nativeView;

			nativeView.TransferbindablePropertiesToWrapper(this);
		}

		public GetDesiredSizeDelegate GetDesiredSizeDelegate { get; }

		public LayoutSubviewsDelegate LayoutSubViews { get; set; }

		public UIView PlatformView { get; }

		public SizeThatFitsDelegate SizeThatFitsDelegate { get; set; }

		protected override void OnBindingContextChanged()
		{
			PlatformView.SetBindingContext(BindingContext, nv => nv.Subviews);
			base.OnBindingContextChanged();
		}
	}
}