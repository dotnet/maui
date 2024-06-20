using System;
using System.Formats.Asn1;
using System.Linq;
using CoreAnimation;
using Microsoft.Maui.Graphics;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorderView, ContentView>
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

		protected override void ConnectHandler(ContentView platformView)
		{
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(ContentView platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.ClearSubviews();
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			PlatformView.View = VirtualView;
			PlatformView.CrossPlatformLayout = VirtualView;
		}

		static partial void UpdateContent(IBorderHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			// Cleanup the old view when reused
			var platformView = handler.PlatformView;
			platformView.ClearSubviews();

			if (handler.VirtualView.PresentedContent is IView content)
			{
				var platformContent = content.ToPlatform(handler.MauiContext);
				platformContent.Tag = ContentView.ContentTag;
				platformView.AddSubview(platformContent);
			}
		}
	}
}