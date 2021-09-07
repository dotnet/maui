using System;
using System.Linq;
using NativeView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler : ViewHandler<IContentView, ContentView>
	{
		protected override ContentView CreateNativeView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(ContentView)}");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

			return new ContentView
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			NativeView.View = view;
			NativeView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			NativeView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		void UpdateContent()
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			//Cleanup the old view when reused
			var oldChildren = NativeView.Subviews.ToList();
			oldChildren.ForEach(x => x.RemoveFromSuperview());

			if (VirtualView.PresentedContent is IView view)
				NativeView.AddSubview(view.ToNative(MauiContext));
		}

		public static void MapContent(ContentViewHandler handler, IContentView page)
		{
			handler.UpdateContent();
		}

		public static void MapFrame(ContentViewHandler handler, IContentView view)
		{
			ViewHandler.MapFrame(handler, view, null);

			// TODO MAUI: Currently the background layer frame is tied to the layout system
			// which needs to be investigated more
			handler.NativeView?.UpdateBackgroundLayerFrame();
		}
	}
}
