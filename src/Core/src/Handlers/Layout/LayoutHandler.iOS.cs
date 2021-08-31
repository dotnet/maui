using System;
using UIKit;
using NativeView = UIKit.UIView;

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
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange,
			};

			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.View = view;
			NativeView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			NativeView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;

			// Remove any previous children 
			var oldChildren = NativeView.Subviews;
			foreach (var child in oldChildren)
			{
				child.RemoveFromSuperview();
			}

			foreach (var child in VirtualView)
			{
				NativeView.AddSubview(child.ToNative(MauiContext));
			}
		}

		public void Add(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.AddSubview(child.ToNative(MauiContext));
		}

		public void Remove(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child?.Handler?.NativeView is NativeView childView)
			{
				childView.RemoveFromSuperview();
			}
		}

		void Clear(UIView nativeView)
		{
			if (nativeView == null)
			{
				return;
			}

			var subViews = nativeView.Subviews;

			foreach (var subView in subViews)
			{
				subView.RemoveFromSuperview();
			}
		}

		public void Clear()
		{
			Clear(NativeView);
		}

		public void Insert(int index, IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.InsertSubview(child.ToNative(MauiContext), index);
		}

		public void Update(int index, IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var existing = NativeView.Subviews[index];
			existing.RemoveFromSuperview();
			NativeView.InsertSubview(child.ToNative(MauiContext), index);
			NativeView.SetNeedsLayout();
		}

		protected override void DisconnectHandler(LayoutView nativeView)
		{
			base.DisconnectHandler(nativeView);
			Clear(nativeView);
		}
	}
}
