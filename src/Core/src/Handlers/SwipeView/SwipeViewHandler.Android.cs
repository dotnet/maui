using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
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
			handler.TypedNativeView.UpdateIsSwipeEnabled();
			ViewHandler.MapIsEnabled(handler, swipeView);
		}

		public static void MapBackground(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			// TODO JAVIER is this right? I wasn't sure exactly how to combine the two
			// UpdateBackgroundColor and  UpdateBackground methods inside SwipeViewRenderer
			handler.TypedNativeView.Control?.SetWindowBackground();
			ViewHandler.MapBackground(handler, swipeView);
		}

		public static void MapSwipeTransitionMode(ISwipeViewHandler handler, ISwipeView swipeView)
		{
			handler.TypedNativeView.UpdateSwipeTransitionMode();
		}
	}
}