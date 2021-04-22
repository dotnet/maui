using System;
using Microsoft.Maui.Graphics;
using UIKit;

#if __IOS__ || IOS || MACCATALYST
using NativeView = UIKit.UIWindow;
#else
using NativeView = AppKit.NSView;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ViewHandler<IWindow, WindowView>
	{
		protected override WindowView CreateNativeView()
		{
			if (VirtualView == null)
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");

			return new WindowView
			{
				CrossPlatformMeasure = VirtualView.Measure,
				CrossPlatformArrange = VirtualView.Arrange,
			};
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.Measure;
			NativeView.CrossPlatformArrange = VirtualView.Arrange;


			NativeView.RootViewController = new UIViewController
			{
				View = VirtualView.Page.ToNative(VirtualView.MauiContext)
			};
		}
	}
}
