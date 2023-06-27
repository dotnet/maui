using System;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemViewHandler : ViewHandler<ISwipeItemView, ContentPanel>, ISwipeItemViewHandler
	{
		protected override ContentPanel CreatePlatformView()
		{
			return new ContentPanel
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
		}

		public static void MapContent(ISwipeItemViewHandler handler, ISwipeItemView page)
		{
			if (handler is SwipeItemViewHandler platformHandler)
				platformHandler.UpdateContent();
		}

		public static void MapVisibility(ISwipeItemViewHandler handler, ISwipeItemView view)
			=> handler.PlatformView.UpdateVisibility(view.Visibility);

		// TODO: NET8 make this public
		internal static void MapIsEnabled(ISwipeItemViewHandler handler, ISwipeItemView view)
			=> handler.PlatformView.UpdateIsEnabled(view);

		void UpdateContent()
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			// Cleanup the old view when reused
			PlatformView.Children.Clear();

			if (VirtualView.PresentedContent is IView view)
				PlatformView.Children.Add(view.ToPlatform(MauiContext));
		}
	}
}