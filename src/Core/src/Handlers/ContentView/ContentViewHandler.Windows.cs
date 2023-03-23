using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler : ViewHandler<IContentView, ContentPanel>
	{
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		static void UpdateContent(IContentViewHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			handler.PlatformView.Children.Clear();

			if (handler.VirtualView.PresentedContent is IView view)
				handler.PlatformView.Children.Add(view.ToPlatform(handler.MauiContext));
		}

		protected override ContentPanel CreatePlatformView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
			}

			var view = new ContentPanel
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};

			return view;
		}

		public static void MapContent(IContentViewHandler handler, IContentView page)
		{
			UpdateContent(handler);
		}

		protected override void DisconnectHandler(ContentPanel platformView)
		{
			platformView.CrossPlatformMeasure = null;
			platformView.CrossPlatformArrange = null;
			base.DisconnectHandler(platformView);
		}
	}
}
