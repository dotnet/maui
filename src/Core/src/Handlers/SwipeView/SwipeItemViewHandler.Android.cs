using System;
using Android.Views;
using Microsoft.Maui.Graphics;
using AButton = AndroidX.AppCompat.Widget.AppCompatButton;
using ATextAlignment = Android.Views.TextAlignment;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public class SwipeItemViewHandler : ViewHandler<ISwipeItemView, ContentViewGroup>
	{

		public static IPropertyMapper<ISwipeItemView, SwipeItemViewHandler> Mapper = new PropertyMapper<ISwipeItemView, SwipeItemViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISwipeItemView.Content)] = MapContent,
			[nameof(ISwipeItemView.Visibility)] = MapVisibility
		};

		public static CommandMapper<ISwipeItemView, ISwipeViewHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
		{
		};

		public SwipeItemViewHandler() : base(Mapper, CommandMapper)
		{

		}

		protected SwipeItemViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper ?? CommandMapper)
		{
		}

		public SwipeItemViewHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{

		}

		protected override ContentViewGroup CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a ContentViewGroup");
			}

			var viewGroup = new ContentViewGroup(Context)
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};

			return viewGroup;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		void UpdateContent()
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			PlatformView.RemoveAllViews();

			if (VirtualView.PresentedContent is IView view)
				PlatformView.AddView(view.ToPlatform(MauiContext));
		}

		public static void MapContent(SwipeItemViewHandler handler, ISwipeItemView page)
		{
			handler.UpdateContent();
		}

		public static void MapVisibility(SwipeItemViewHandler handler, ISwipeItemView view)
		{
			var swipeView = handler.PlatformView?.Parent.GetParentOfType<MauiSwipeView>();
			if (swipeView != null)
				swipeView.UpdateIsVisibleSwipeItem(view);
		}

		protected override void DisconnectHandler(ContentViewGroup nativeView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its chidren
			nativeView.RemoveAllViews();
			base.DisconnectHandler(nativeView);
		}
	}
}
