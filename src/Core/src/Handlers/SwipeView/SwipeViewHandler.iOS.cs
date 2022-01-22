using System;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ViewHandler<ISwipeView, MauiSwipeView>
	{
		protected override MauiSwipeView CreateNativeView()
		{
			return new MauiSwipeView
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange,
				Element = VirtualView
			};
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");

			NativeView.Element = VirtualView;
			NativeView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			NativeView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		public static void MapContent(ISwipeViewHandler handler, ISwipeView view)
		{
			_ = handler.NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.TypedVirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			handler.TypedNativeView.UpdateContent(view, handler.MauiContext);
		}

		public static void MapIsEnabled(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			handler.TypedNativeView.UpdateIsSwipeEnabled(swipeView);
			ViewHandler.MapIsEnabled(handler, swipeView);
		}

		public static void MapSwipeTransitionMode(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			handler.TypedNativeView.UpdateSwipeTransitionMode(swipeView.SwipeTransitionMode);
		}

		public static void MapRequestOpen(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewOpenRequest request)
			{
				return;
			}

			handler.TypedNativeView.ProgrammaticallyOpenSwipeItem(request.OpenSwipeItem, request.Animated);
		}

		public static void MapRequestClose(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewCloseRequest request)
			{
				return;
			}
			handler.TypedNativeView.ResetSwipe(request.Animated);
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