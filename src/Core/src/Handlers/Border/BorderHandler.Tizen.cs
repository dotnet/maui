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

		public static void MapContent(IBorderHandler handler, IBorderView border)
		{
			if (handler is BorderHandler borderHandler)
				borderHandler.UpdateContent();
		}

		void UpdateContent()
		{
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			PlatformView.Children.Clear();
			_contentHandler?.Dispose();
			_contentHandler = null;

			if (VirtualView.PresentedContent is IView view)
			{
				PlatformView.Children.Add(view.ToPlatform(MauiContext));
				if (view.Handler is IPlatformViewHandler thandler)
				{
					_contentHandler = thandler;
				}
			}
		}
	}
}
