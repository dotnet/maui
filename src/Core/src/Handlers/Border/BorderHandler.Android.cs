using System;
using Android.Views;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorderView, ContentViewGroup>
	{
		protected override ContentViewGroup CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a ContentViewGroup");
			}

			var viewGroup = new ContentViewGroup(Context)
			{
				CrossPlatformLayout = VirtualView
			};

			// We only want to use a hardware layer for the entering view because its quite likely
			// the view will invalidate several times the Drawable (Draw).
			viewGroup.SetLayerType(LayerType.Hardware, null);

			return viewGroup;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.CrossPlatformLayout = VirtualView;
		}

		static partial void UpdateContent(IBorderHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			handler.PlatformView.RemoveAllViews();

			if (handler.VirtualView.PresentedContent is IView view)
				handler.PlatformView.AddView(view.ToPlatform(handler.MauiContext));
		}

		public static partial void MapHeight(IBorderHandler handler, IBorderView border)
		{
			handler.PlatformView?.UpdateHeight(border);
			handler.PlatformView?.InvalidateBorderStrokeBounds();
		}

		public static partial void MapWidth(IBorderHandler handler, IBorderView border)
		{
			handler.PlatformView?.UpdateWidth(border);
			handler.PlatformView?.InvalidateBorderStrokeBounds();
		}

		protected override void DisconnectHandler(ContentViewGroup platformView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its children
			platformView.RemoveAllViews();

			base.DisconnectHandler(platformView);
		}
	}
}