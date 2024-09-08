using System;
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ViewHandler<ILayout, LayoutViewGroup>
	{
		protected override LayoutViewGroup CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutViewGroup");
			}

			var viewGroup = new LayoutViewGroup(Context!)
			{
				CrossPlatformLayout = VirtualView
			};

			// .NET MAUI layouts should not impose clipping on their children	
			viewGroup.SetClipChildren(false);

			return viewGroup;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			var platformView = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var virtualView = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			var mauiContext = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			platformView.CrossPlatformLayout = virtualView;

			platformView.RemoveAllViews();

			var targetIndex = platformView.ChildCount;
			foreach (var child in virtualView.OrderByZIndex())
			{
				var childHandler = ((IElement)child).ToHandler(mauiContext);

				if (childHandler is IHeadlessLayoutHandler headlessLayoutHandler)
				{
					headlessLayoutHandler.CreateSubviews(ref targetIndex);
					continue;
				}

				platformView.AddView(childHandler.ToPlatform());
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
			platformView.AddView(childPlatformView, targetIndex);
		}

		public void Remove(IView child)
		{
			var mauiContext = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var platformView = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			var childHandler = ((IElement)child).ToHandler(mauiContext);

			if (childHandler is IHeadlessLayoutHandler headlessLayoutHandler)
			{
				headlessLayoutHandler.Clear();
				return;
			}

			platformView.RemoveView(childHandler.ToPlatform());
		}

		static void Clear(LayoutViewGroup platformView)
		{
			if (platformView.IsDisposed() == false)
			{
				platformView.RemoveAllViews();
			}
		}

		public void Clear()
		{
			Clear(PlatformView);
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
			platformView.AddView(childPlatformView, targetIndex);
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

			platformView.RemoveView(child.ToPlatform(mauiContext));
			platformView.AddView(child.ToPlatform(mauiContext), targetIndex);
		}

		public void UpdateZIndex(IView child)
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			EnsureZIndexOrder(child);
		}

		protected override void DisconnectHandler(LayoutViewGroup platformView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its children
			Clear(platformView);
			base.DisconnectHandler(platformView);
		}

		void EnsureZIndexOrder(IView child)
		{
			var platformView = PlatformView;
			if (platformView.ChildCount == 0)
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

			AView platformChildView = child.ToPlatform(mauiContext);
			var currentIndex = platformView.IndexOfChild(platformChildView);

			if (currentIndex == -1)
			{
				return;
			}

			if (currentIndex != targetIndex)
			{
				platformView.RemoveViewAt(currentIndex);
				platformView.AddView(platformChildView, targetIndex);
			}
		}

		public static partial void MapBackground(ILayoutHandler handler, ILayout layout)
		{
			handler.PlatformView?.UpdateBackground(layout);
		}

		public static partial void MapInputTransparent(ILayoutHandler handler, ILayout layout)
		{
			if (handler.PlatformView is LayoutViewGroup layoutViewGroup)
			{
				layoutViewGroup.InputTransparent = layout.InputTransparent;
			}
		}
	}
}
