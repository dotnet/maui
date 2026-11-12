using System;

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

			// No hardware layer: a SurfaceView in a layer, e.g. a video, isn't visible on Android 10 and 11, and the stroke
			// isn't drawn around it on Android 12 (https://github.com/dotnet/maui/issues/40186)
			return new ContentViewGroup(Context)
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

		static partial void UpdateContent(IBorderHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			handler.PlatformView.RemoveAllViews();

			if (handler.VirtualView.PresentedContent is IView view)
			{
				var platformView = view.ToPlatform(handler.MauiContext);
				// Ensure the view is detached from any existing parent before adding it
				platformView.RemoveFromParent();
				handler.PlatformView.AddView(platformView);
			}
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