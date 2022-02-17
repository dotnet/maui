using System;

namespace Microsoft.Maui.Handlers
{
	public class SwipeItemViewHandler : ViewHandler<ISwipeItemView, ContentView>
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

		protected override ContentView CreatePlatformView()
		{
			return new ContentView
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};
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
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			// Cleanup the old view when reused
			PlatformView.ClearSubviews();

			if (VirtualView.PresentedContent is IView view)
				PlatformView.AddSubview(view.ToPlatform(MauiContext));
		}

		public static void MapContent(SwipeItemViewHandler handler, ISwipeItemView page)
		{
			handler.UpdateContent();
		}

		public static void MapVisibility(SwipeItemViewHandler handler, ISwipeItemView view)
		{
			var swipeView = handler.PlatformView.GetParentOfType<MauiSwipeView>();
			if (swipeView != null)
				swipeView.UpdateIsVisibleSwipeItem(view);

			handler.PlatformView.UpdateVisibility(view.Visibility);
		}
	}
}
