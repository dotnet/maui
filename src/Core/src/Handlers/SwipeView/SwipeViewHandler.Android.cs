using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ViewHandler<ISwipeView, MauiSwipeView>
	{
		MauiSwipeView? GetMauiSwipeView() => PlatformView as MauiSwipeView;
		static MauiSwipeView? GetMauiSwipeView(ISwipeViewHandler handler) => handler.PlatformView as MauiSwipeView;

		protected override MauiSwipeView CreatePlatformView()
		{
			var returnValue = new MauiSwipeView(Context)
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};

			returnValue.SetElement(VirtualView);
			return returnValue;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
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

		public static void MapContent(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			GetMauiSwipeView(handler)?.UpdateContent();
		}

		public static void MapIsEnabled(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			GetMauiSwipeView(handler)?.UpdateIsSwipeEnabled(swipeView.IsEnabled);
			ViewHandler.MapIsEnabled(handler, swipeView);
		}

		public static void MapBackground(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			if (swipeView.Background == null)
				GetMauiSwipeView(handler)?.Control?.SetWindowBackground();
			else
				ViewHandler.MapBackground(handler, swipeView);
		}

		public static void MapSwipeTransitionMode(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			GetMauiSwipeView(handler)?.UpdateSwipeTransitionMode(swipeView.SwipeTransitionMode);
		}

		public static void MapRequestOpen(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewOpenRequest request)
			{
				return;
			}

			GetMauiSwipeView(handler)?.OnOpenRequested(request);
		}

		public static void MapRequestClose(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewCloseRequest request)
			{
				return;
			}

			GetMauiSwipeView(handler)?.OnCloseRequested(request);
		}
	}
}
