using System;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler : ViewHandler<IContentView, ContentView>
	{
		protected override ContentView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(ContentView)}");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

			return new ContentView
			{
				CrossPlatformLayout = VirtualView
			};
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			PlatformView.View = view;
			PlatformView.CrossPlatformLayout = VirtualView;
		}

		static void UpdateContent(IContentViewHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			// Cleanup the old view when reused
			handler.PlatformView.ClearSubviews();

			if (handler.VirtualView.PresentedContent is IView view)
			{
				var platformView = view.ToPlatform(handler.MauiContext);
				handler.PlatformView.AddSubview(platformView);

				if (view.FlowDirection == FlowDirection.MatchParent)
				{
					platformView.UpdateFlowDirection(view);
				}

				// we need to trigger an invalidation of ancestor measures after the view has been added
				// so that it can walk up the hierarchy
				platformView.InvalidateAncestorsMeasures();
			}
			else
			{
				if (handler.VirtualView.PresentedContent is null)
				{
					// When content is removed, we need to invalidate measures so the ContentView can resize to 0x0
					// Only invalidate if the ContentView previously had content (optimization to avoid unnecessary invalidation)
					// Invalidate both the ContentView itself and its ancestors
					if (handler.PlatformView is IPlatformMeasureInvalidationController controller)
					{
						controller.InvalidateMeasure(false);
					}
					handler.PlatformView.InvalidateAncestorsMeasures();
				}
			
			}
		}		
	public static partial void MapContent(IContentViewHandler handler, IContentView page)
		{
			UpdateContent(handler);
		}

		protected override void DisconnectHandler(ContentView platformView)
		{
			platformView.CrossPlatformLayout = null;
			platformView.RemoveFromSuperview();
			base.DisconnectHandler(platformView);
		}
	}
}
