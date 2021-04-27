using System;
using System.Linq;

#if __IOS__ || IOS || MACCATALYST
using NativeView = UIKit.UIView;
using UIKit;
#else
using NativeView = AppKit.NSView;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ViewHandler<IPage, PageView>, INativeViewHandler
	{
		PageViewController? _pageViewController;
		UIViewController? INativeViewHandler.ViewController => _pageViewController;

		protected override PageView CreateNativeView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

			_pageViewController = new PageViewController(VirtualView, this.MauiContext);

			if (_pageViewController.CurrentNativeView == null)
				_pageViewController.LoadNativeView();

			if (_pageViewController.CurrentNativeView is PageView pv)
				return pv;

			throw new InvalidOperationException($"PageViewController.View must be a PageView");
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			//Cleanup the old view when reused
			var oldChildren = NativeView.Subviews.ToList();
			oldChildren.ForEach(x => x.RemoveFromSuperview());

			NativeView.CrossPlatformArrange = VirtualView.Arrange;
			NativeView.AddSubview(VirtualView.Content.ToNative(MauiContext));
		}

		public static void MapTitle(PageHandler handler, IPage page)
		{
			if(handler._pageViewController != null)
				handler._pageViewController.Title = page.Title;
		}
	}
}
