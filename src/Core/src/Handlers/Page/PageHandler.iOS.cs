using System;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ContentViewHandler, IPlatformViewHandler
	{
		protected override ContentView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a LayoutView");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} cannot be null");

			if (ViewController == null)
				ViewController = new PageViewController(VirtualView, MauiContext);

			if (ViewController is PageViewController pc && pc.CurrentPlatformView is ContentView pv)
				return pv;

			if (ViewController.View is ContentView cv)
				return cv;

			throw new InvalidOperationException($"PageViewController.View must be a {nameof(ContentView)}");
		}

		public static void MapBackground(IPageHandler handler, IContentView page)
		{
			if (handler is IPlatformViewHandler platformViewHandler && platformViewHandler.ViewController is not null)
			{
				var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
				platformViewHandler.ViewController.View?.UpdateBackground(page, provider);
			}
		}

		internal static void MapHomeIndicatorAutoHidden(IPageHandler handler, IContentView page)
		{
			if (handler is IPlatformViewHandler platformViewHandler && platformViewHandler.ViewController is not null)
			{
				platformViewHandler.ViewController.SetNeedsUpdateOfHomeIndicatorAutoHidden();
			}
		}

		internal static void MapPrefersStatusBarHiddenMode(IPageHandler handler, IContentView page)
		{
			if (handler is IPlatformViewHandler platformViewHandler && platformViewHandler.ViewController is not null)
			{
				platformViewHandler.ViewController.SetNeedsStatusBarAppearanceUpdate();
			}
		}

		public static void MapTitle(IPageHandler handler, IContentView page)
		{
			if (handler is IPlatformViewHandler platformViewHandler && platformViewHandler.ViewController is not null)
			{
				platformViewHandler.ViewController.UpdateTitle(page);
			}
		}
	}
}
