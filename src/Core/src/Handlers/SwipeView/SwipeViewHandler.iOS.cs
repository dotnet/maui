using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ViewHandler<ISwipeView, MauiSwipeView>
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

		protected override MauiSwipeView CreatePlatformView() => new() { CrossPlatformLayout = VirtualView };

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.CrossPlatformLayout = VirtualView;
			PlatformView.View = VirtualView;
		}

		public static void MapContent(ISwipeViewHandler handler, ISwipeView view)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			handler.PlatformView.UpdateContent(view, handler.MauiContext);
		}

		public static void MapIsEnabled(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			handler.PlatformView.UpdateIsSwipeEnabled(swipeView);
			ViewHandler.MapIsEnabled(handler, swipeView);
		}

		public static void MapSwipeTransitionMode(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			handler.PlatformView.UpdateSwipeTransitionMode(swipeView.SwipeTransitionMode);
		}

		public static void MapRequestOpen(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewOpenRequest request)
			{
				return;
			}

			handler.PlatformView.ProgrammaticallyOpenSwipeItem(request.OpenSwipeItem, request.Animated);
		}

		public static void MapRequestClose(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewCloseRequest request)
			{
				return;
			}
			handler.PlatformView.ResetSwipe(request.Animated);
		}

		public static void MapLeftItems(ISwipeViewHandler handler, ISwipeView view)
		{
		}
		public static void MapTopItems(ISwipeViewHandler handler, ISwipeView view)
		{
		}
		public static void MapRightItems(ISwipeViewHandler handler, ISwipeView view)
		{
		}
		public static void MapBottomItems(ISwipeViewHandler handler, ISwipeView view)
		{
		}
	}
}