using System;

#if __IOS__ || IOS || MACCATALYST
using NativeView = UIKit.UIView;
#else
using NativeView = AppKit.NSView;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutView>
	{
		protected override LayoutView CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutViewGroup");
			}

			var view = new LayoutView
			{
				CrossPlatformMeasure = VirtualView.Measure,
				CrossPlatformArrange = VirtualView.Arrange,
			};

			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = View ?? throw new InvalidOperationException($"{nameof(View)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			View.CrossPlatformMeasure = VirtualView.Measure;
			View.CrossPlatformArrange = VirtualView.Arrange;

			foreach (var child in VirtualView.Children)
			{
				View.AddSubview(child.ToNative(MauiContext));
			}
		}

		public void Add(IView child)
		{
			_ = View ?? throw new InvalidOperationException($"{nameof(View)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			View.AddSubview(child.ToNative(MauiContext));
			View.SetNeedsLayout();
		}

		public void Remove(IView child)
		{
			_ = View ?? throw new InvalidOperationException($"{nameof(View)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child?.Handler?.View is NativeView nativeView)
			{
				nativeView.RemoveFromSuperview();
				View.SetNeedsLayout();
			}
		}
	}
}
