using System;
using System.Collections.Generic;
using UIKit;


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

		public static void MapTitle(IPageHandler handler, IContentView page)
		{
			if (handler is IPlatformViewHandler invh && invh.ViewController != null)
			{
				if (page is ITitledElement titled)
				{
					invh.ViewController.Title = titled.Title;
				}
			}
		}
	}
}
