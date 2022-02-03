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

			var viewGroup = new ContentViewGroup(Context)
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};

			// We only want to use a hardware layer for the entering view because its quite likely
			// the view will invalidate several times the Drawable (Draw).
			viewGroup.SetLayerType(Android.Views.LayerType.Hardware, null);

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

		public static void MapContent(BorderHandler handler, IBorderView border)
		{
			handler.UpdateContent();
		}

		protected override void DisconnectHandler(ContentViewGroup nativeView)
		{
			// If we're being disconnected from the xplat element, then we should no longer be managing its chidren
			nativeView.RemoveAllViews();

			base.DisconnectHandler(nativeView);
		}
	}
}
