using System;
using ContentViewGroup = Microsoft.Maui.Platform.ContentViewGroup;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorderView, ContentViewGroup>
	{
		IPlatformViewHandler? _contentHandler;

		protected override ContentViewGroup CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a Page");

			var view = new ContentViewGroup(VirtualView)
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};
			return view;
		}

		protected override void SetupContainer()
		{
			base.SetupContainer();
			ContainerView?.UpdateBorder(VirtualView);
			ContainerView?.UpdateBackground(VirtualView.Background);
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		static partial void UpdateContent(IBorderHandler handler)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			handler.PlatformView.Children.Clear();
			if (handler is BorderHandler borderHandler)
			{
				borderHandler._contentHandler?.Dispose();
				borderHandler._contentHandler = null;
			}

			if (handler.VirtualView.PresentedContent is IView view)
			{
				handler.PlatformView.Children.Add(view.ToPlatform(handler.MauiContext));
				if (view.Handler is IPlatformViewHandler thandler && handler is BorderHandler alsoBorderHandler)
				{
					alsoBorderHandler._contentHandler = thandler;
				}
			}
		}
	}
}
