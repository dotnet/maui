using System;
using UIKit;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorderView, ContentView>
	{
		protected override void SetupContainer()
		{
			base.SetupContainer();

			// When a WrapperView is created, the child (PlatformView) is moved inside it.
			// Reset the child's transform to identity to prevent transform compounding,
			// since the WrapperView will handle the transform for the entire container.
			if (ContainerView is WrapperView)
			{
				PlatformView.ResetLayerTransform();
			}
		}

		protected override ContentView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(ContentView)}");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

			return new ContentView
			{
				CrossPlatformLayout = VirtualView
			};
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

				// If the content is a UIScrollView, we need a container to handle masks and clip shapes effectively.
				if (platformContent is UIScrollView)
				{
					var containerView = new UIView
					{
						AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth
					};

					containerView.Tag = ContentView.ContentTag;
					containerView.AddSubview(platformContent);
					platformView.AddSubview(containerView);
				}
				else
				{
					platformContent.Tag = ContentView.ContentTag;
					platformView.AddSubview(platformContent);
				}
			}
		}
	}
}