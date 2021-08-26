using System;
using System.Linq;
using UIKit;
using NativeView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ViewHandler<IView, PageView>, INativeViewHandler
	{

		public new partial class Factory : ViewHandler.Factory
		{
			public virtual PageViewController CreateViewController(PageHandler pageHandler, IView scrollView)
			{
				return new PageViewController(scrollView, pageHandler);
			}

			public virtual PageView CreateNativeView(PageHandler pageHandler, IView scrollView)
			{
				return new PageView
				{
					CrossPlatformArrange = scrollView.Arrange,
					CrossPlatformMeasure = scrollView.Measure
				};
			}
		}

		PageViewController? _pageViewController;
		UIViewController? INativeViewHandler.ViewController => _pageViewController;

		protected override PageView CreateNativeView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

			_pageViewController = (PageViewController)FactoryMapper[nameof(Factory.CreateViewController)].Invoke(this, VirtualView, null)!;

			if (_pageViewController.CurrentNativeView is PageView pv)
				return pv;

			throw new InvalidOperationException($"PageViewController.View must be a PageView");
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			NativeView.View = view;
			NativeView.CrossPlatformArrange = VirtualView.Arrange;
		}

		void UpdateContent()
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			//Cleanup the old view when reused
			var oldChildren = NativeView.Subviews.ToList();
			oldChildren.ForEach(x => x.RemoveFromSuperview());

			if (VirtualView is IContentView cv && cv.Content is IView view)
				NativeView.AddSubview(view.ToNative(MauiContext));
		}

		public static void MapTitle(PageHandler handler, IView page)
		{
			if (handler._pageViewController != null && page is ITitledElement titled)
				handler._pageViewController.Title = titled.Title;
		}

		public static void MapContent(PageHandler handler, IView page)
		{
			handler.UpdateContent();
		}

		public static void MapFrame(PageHandler handler, IView view)
		{
			ViewHandler.MapFrame(handler, view, null);

			// TODO MAUI: Currently the background layer frame is tied to the layout system
			// which needs to be investigated more
			handler.NativeView?.UpdateBackgroundLayerFrame();
		}
	}
}
