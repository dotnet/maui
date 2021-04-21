using System;

#if __IOS__ || IOS || MACCATALYST
using NativeView = UIKit.UIView;
#else
using NativeView = AppKit.NSView;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ViewHandler<IWindow, PageView>
	{
		protected override PageView CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
			}

			var view = new PageView
			{
				CrossPlatformArrange = VirtualView.Arrange,
			};

			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.CrossPlatformArrange = VirtualView.Arrange;
			NativeView.AddSubview(VirtualView.Page.ToNative(MauiContext));
		}
	}
}
