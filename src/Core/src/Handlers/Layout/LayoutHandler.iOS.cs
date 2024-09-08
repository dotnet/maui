using System;
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

			var platformView = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var virtualView = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			var mauiContext = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			
			platformView.View = view;
			platformView.CrossPlatformLayout = virtualView;

			// Remove any previous children 
			platformView.ClearSubviews();

			var targetIndex = platformView.Subviews.Length;
			foreach (var child in virtualView.OrderByZIndex())
			{
				var childHandler = ((IElement)child).ToHandler(mauiContext);

				if (childHandler is IHeadlessLayoutHandler headlessLayoutHandler)
				{
					headlessLayoutHandler.CreateSubviews(ref targetIndex);
					continue;
				}

				platformView.AddSubview(childHandler.ToPlatform());
				++targetIndex;
			}
		}

		public void Add(IView child)
		{
			var mauiContext = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var platformView = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var virtualView = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			var childHandler = ((IElement)child).ToHandler(mauiContext);

			var targetIndex = virtualView.GetLayoutHandlerIndex(child);
			if (childHandler is IHeadlessLayoutHandler headlessLayoutHandler)
			{
				headlessLayoutHandler.CreateSubviews(ref targetIndex);
				return;
			}

			var childPlatformView = childHandler.ToPlatform();
			platformView.InsertSubview(childPlatformView, targetIndex);

			if (child.FlowDirection == FlowDirection.MatchParent)
			{
				childPlatformView.UpdateFlowDirection(child);
			}
		}

		public void Remove(IView child)
		{
			var mauiContext = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			var childHandler = ((IElement)child).ToHandler(mauiContext);

			if (childHandler is IHeadlessLayoutHandler headlessLayoutHandler)
			{
				headlessLayoutHandler.Clear();
				return;
			}

			childHandler.ToPlatform().RemoveFromSuperview();
		}

		public void Clear()
		{
			PlatformView.ClearSubviews();
		}

		public void Insert(int index, IView child)
		{
			var mauiContext = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var platformView = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var virtualView = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			var targetIndex = virtualView.GetLayoutHandlerIndex(child);
			
			var childHandler = ((IElement)child).ToHandler(mauiContext);

			if (childHandler is IHeadlessLayoutHandler headlessLayoutHandler)
			{
				headlessLayoutHandler.CreateSubviews(ref targetIndex);
				return;
			}

			var childPlatformView = childHandler.ToPlatform();
			platformView.InsertSubview(childPlatformView, targetIndex);

			if (child.FlowDirection == FlowDirection.MatchParent)
			{
				childPlatformView.UpdateFlowDirection(child);
			}
		}

		public void Update(int index, IView child)
		{
			var mauiContext = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var virtualView = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			var platformView = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			
			var childHandler = ((IElement)child).ToHandler(mauiContext);

			var targetIndex = virtualView.GetLayoutHandlerIndex(child);
			if (childHandler is IHeadlessLayoutHandler headlessLayoutHandler)
			{
				headlessLayoutHandler.MoveSubviews(targetIndex);
				return;
			}

			child.ToPlatform(mauiContext).RemoveFromSuperview();
			platformView.InsertSubview(child.ToPlatform(mauiContext), targetIndex);
			platformView.SetNeedsLayout();
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
			var platformView = PlatformView;
			if (platformView.Subviews.Length == 0)
			{
				return;
			}

			var mauiContext = MauiContext!;
			var childHandler = ((IElement)child).ToHandler(mauiContext);
			var targetIndex = VirtualView.GetLayoutHandlerIndex(child);

			if (childHandler is IHeadlessLayoutHandler headlessLayoutHandler)
			{
				headlessLayoutHandler.MoveSubviews(targetIndex);
				return;
			}

			PlatformView nativeChildView = child.ToPlatform(mauiContext);
			var currentIndex = platformView.Subviews.IndexOf(nativeChildView);

			if (currentIndex == -1)
			{
				return;
			}

			if (currentIndex != targetIndex)
			{
				platformView.Subviews.RemoveAt(currentIndex);
				platformView.InsertSubview(nativeChildView, targetIndex);
			}
		}

		public static partial void MapBackground(ILayoutHandler handler, ILayout layout)
		{
			handler.PlatformView?.UpdateBackground(layout);
		}
	}
}
