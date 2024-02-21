using System;
using ObjCRuntime;
using UIKit;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutView>
	{
		protected override LayoutView CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutViewGroup");
			}

			return new()
			{
				CrossPlatformLayout = VirtualView
			};
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			PlatformView.View = view;
			PlatformView.CrossPlatformLayout = VirtualView;

			// Remove any previous children 
			PlatformView.ClearSubviews();

			foreach (var child in VirtualView.OrderByZIndex())
			{
				PlatformView.AddSubview(child.ToPlatform(MauiContext));
			}
		}

		public void Add(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			var childPlatformView = child.ToPlatform(MauiContext);
			PlatformView.InsertSubview(childPlatformView, targetIndex);

			if (child.FlowDirection == FlowDirection.MatchParent)
			{
				childPlatformView.UpdateFlowDirection(child);
			}
		}

		public void Remove(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child?.ToPlatform() is PlatformView childView)
			{
				childView.RemoveFromSuperview();
			}
		}

		public void Clear()
		{
			PlatformView.ClearSubviews();
		}

		public void Insert(int index, IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			var childPlatformView = child.ToPlatform(MauiContext);
			PlatformView.InsertSubview(childPlatformView, targetIndex);

			if (child.FlowDirection == FlowDirection.MatchParent)
			{
				childPlatformView.UpdateFlowDirection(child);
			}
		}

		public void Update(int index, IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var existing = PlatformView.Subviews[index];
			existing.RemoveFromSuperview();
			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);
			PlatformView.InsertSubview(child.ToPlatform(MauiContext), targetIndex);
			PlatformView.SetNeedsLayout();
		}

		public void UpdateZIndex(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			EnsureZIndexOrder(child);
		}

		protected override void DisconnectHandler(LayoutView platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.ClearSubviews();
		}

		void EnsureZIndexOrder(IView child)
		{
			if (PlatformView.Subviews.Length == 0)
			{
				return;
			}

			PlatformView nativeChildView = child.ToPlatform(MauiContext!);
			var currentIndex = PlatformView.Subviews.IndexOf(nativeChildView);

			if (currentIndex == -1)
			{
				return;
			}

			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);

			if (currentIndex != targetIndex)
			{
				PlatformView.Subviews.RemoveAt(currentIndex);
				PlatformView.InsertSubview(nativeChildView, targetIndex);
			}
		}

		public static partial void MapBackground(ILayoutHandler handler, ILayout layout)
		{
			handler.PlatformView?.UpdateBackground(layout);
		}
	}
}
