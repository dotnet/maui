using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler : ViewHandler<IContentView, ContentViewGroup>
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

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		public static partial void MapBackground(IContentViewHandler handler, IContentView view)
		{
			handler.UpdateValue(nameof(handler.ContainerView));
			handler.ToPlatform()?.UpdateBackground(view);
		}

		public static partial void MapContent(IContentViewHandler handler, IContentView page)
		{
			if (handler is ContentViewHandler contentViewHandler)
			{
				contentViewHandler.UpdateContent();
			}
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
				PlatformView.SetNeedMeasureUpdate();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_contentHandler?.Dispose();
			base.Dispose(disposing);
		}
	}
}
