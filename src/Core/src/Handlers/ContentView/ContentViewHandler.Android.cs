using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler : ViewHandler<IContentView, ContentViewGroup>
	{
		protected override ContentViewGroup CreateNativeView()
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
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			NativeView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		void UpdateContent()
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			NativeView.RemoveAllViews();

			if (VirtualView.PresentedContent is IView view)
				NativeView.AddView(view.ToNative(MauiContext));
		}

		public static void MapContent(ContentViewHandler handler, IContentView page)
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
