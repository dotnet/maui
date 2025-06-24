using System;
using Android.Content.PM;

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

			// Check if hardware acceleration is enabled for the application
			// If disabled, use software layer to avoid rendering issues
			var layerType = IsHardwareAccelerationEnabled() ? Android.Views.LayerType.Hardware : Android.Views.LayerType.Software;
			
			// We only want to use a hardware layer for the entering view because its quite likely
			// the view will invalidate several times the Drawable (Draw).
			// However, if hardware acceleration is disabled, use software layer instead.
			viewGroup.SetLayerType(layerType, null);

			return viewGroup;
		}

		private bool IsHardwareAccelerationEnabled()
		{
			try
			{
				if (Context?.ApplicationContext?.ApplicationInfo != null)
				{
					// ApplicationInfoFlags.HardwareAccelerated is only available on API 23+
					if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
					{
#pragma warning disable CA1416 // Validate platform compatibility - Already checked above
						return (Context.ApplicationContext.ApplicationInfo.Flags & ApplicationInfoFlags.HardwareAccelerated) != 0;
#pragma warning restore CA1416
					}
					
					// For older API levels, assume hardware acceleration is enabled by default
					// unless explicitly disabled, as hardware acceleration was introduced in API 11
					// and became the default for target SDK 14+
					return true;
				}
			}
			catch
			{
				// If we can't determine the status, default to assuming hardware acceleration
				// is available for backwards compatibility
			}

			return true;
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