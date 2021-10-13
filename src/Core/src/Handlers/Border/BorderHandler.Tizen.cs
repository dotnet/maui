using System;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorder, ContentViewGroup>
	{
		IPlatformViewHandler? _contentHandler;

		protected override ContentViewGroup CreateNativeView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a Page");

<<<<<<< HEAD
			var view = new BorderView(PlatformParent, VirtualView)
=======
			var view = new ContentViewGroup(VirtualView)
>>>>>>> f87d0f187 (Add NUI handler)
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};
			return view;
		}

		protected override void SetupContainer()
		{
			base.SetupContainer();
			PlatformView.ContainerView = ContainerView;
		}

		public override Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return VirtualView.CrossPlatformMeasure(widthConstraint, heightConstraint);
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
