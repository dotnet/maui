using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ViewHandler<ISwipeView, MauiSwipeView>
	{
		protected override MauiSwipeView CreatePlatformView() 
		{ 
			var returnValue = new MauiSwipeView();

			returnValue.SetElement(VirtualView);

			return returnValue;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		}

		public static void MapContent(ISwipeViewHandler handler, ISwipeView view)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");


			handler.PlatformView.UpdateContent();
		}

		public static void MapSwipeTransitionMode(ISwipeViewHandler handler, ISwipeView swipeView)
		{
		}

		public static void MapRequestOpen(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewOpenRequest request)
			{
				return;
			}

			handler.PlatformView.OnOpenRequested(request);
		}

		public static void MapRequestClose(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewCloseRequest request)
			{
				return;
			}

			handler.PlatformView.OnCloseRequested(request);
		}

		protected override void ConnectHandler(MauiSwipeView platformView)
		{
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiSwipeView platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.Dispose();
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