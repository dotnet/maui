using System;
using PlatformView = Microsoft.Maui.Platform.ContentView;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemViewHandler : ViewHandler<ISwipeItemView, ContentView>, ISwipeItemViewHandler
	{
		protected override ContentView CreatePlatformView()
		{
			return new ContentView
			{
				CrossPlatformLayout = VirtualView
			};
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.CrossPlatformLayout = VirtualView;
			PlatformView.View = VirtualView;
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

		public static void MapContent(ISwipeItemViewHandler handler, ISwipeItemView page)
		{
			if (handler is SwipeItemViewHandler platformHandler)
				platformHandler.UpdateContent();
		}

		public static void MapVisibility(ISwipeItemViewHandler handler, ISwipeItemView view)
		{
			var swipeView = handler.PlatformView.GetParentOfType<MauiSwipeView>();
			swipeView?.UpdateIsVisibleSwipeItem(view);

			handler.PlatformView.UpdateVisibility(view.Visibility);
		}
	}
}
