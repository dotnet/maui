using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ViewHandler<ISwipeView, MauiSwipeView>
	{
		protected override MauiSwipeView CreateNativeView()
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
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			NativeView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		public static void MapContent(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			_ = handler.NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.TypedVirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			handler.TypedNativeView.UpdateContent();
		}

		public static void MapIsEnabled(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			handler.TypedNativeView.UpdateIsSwipeEnabled(swipeView.IsEnabled);
			ViewHandler.MapIsEnabled(handler, swipeView);
		}

		public static void MapBackground(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			if (swipeView.Background == null)
				handler.TypedNativeView.Control?.SetWindowBackground();
			else
				ViewHandler.MapBackground(handler, swipeView);
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

			handler.TypedNativeView.OnOpenRequested(request);
		}

		public static void MapRequestClose(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewCloseRequest request)
			{
				return;
			}

			handler.TypedNativeView.OnCloseRequested(request);
		}
	}
}
