using System;
using ObjCRuntime;
using UIKit;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutView>
	{
		protected override void SetupContainer()
		{
			base.SetupContainer();

			// When a WrapperView is created, the child (PlatformView) is moved inside it.
			// Reset the child's transform to identity to prevent transform compounding,
			// since the WrapperView will handle the transform for the entire container.
			if (ContainerView is WrapperView)
			{
				PlatformView.ResetLayerTransform();
			}
		}

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

			PlatformView.InvalidateAncestorsMeasures();
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

			PlatformView.InvalidateAncestorsMeasures();
		}

		public void Remove(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			if (child?.ToPlatform() is PlatformView childView)
			{
				childView.RemoveFromSuperview();
			}

			PlatformView.InvalidateAncestorsMeasures();
		}

		public void Clear()
		{
			PlatformView.ClearSubviews();
			PlatformView.InvalidateAncestorsMeasures();
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

			PlatformView.InvalidateAncestorsMeasures();
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
			PlatformView.InvalidateAncestorsMeasures();
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
				PlatformView.InvalidateAncestorsMeasures();
			}
		}

		public static partial void MapBackground(ILayoutHandler handler, ILayout layout)
		{
			handler.PlatformView?.UpdateBackground(layout);
		}
	}
}
