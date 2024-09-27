using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UIWindow>
	{
		protected override void ConnectHandler(UIWindow platformView)
		{
			base.ConnectHandler(platformView);

			UpdateVirtualViewFrame(platformView);
		}
		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.PlatformView.UpdateTitle(window);

#if MACCATALYST
		internal static void MapTitleBar(IWindowHandler handler, IWindow window)
		{
			handler.PlatformView.UpdateTitleBar(window, handler.MauiContext);
			// MapContent(handler, window);
		}
#endif

		// TODO maybe this MapContent needs to be called when we change the UpdateTitleBar
		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = window.Content.ToUIViewController(handler.MauiContext);

			// Create a container view controller
			var containerViewController = new UIViewController();

#if MACCATALYST
			var mauiTitleBar = window.TitleBar?.ToPlatform(handler.MauiContext);

			// Add the native content as a child view controller
			containerViewController.AddChildViewController(nativeContent);

			if (nativeContent.View is null || mauiTitleBar is null)
				return;

			if (containerViewController?.View is UIView cVCView)
			{
				cVCView.AddSubview(nativeContent.View);
				nativeContent.DidMoveToParentViewController(containerViewController);

				cVCView.AddSubview(mauiTitleBar);

				var items = window.TitleBar?.PassthroughElements;
				if (items is not null)
				{
					foreach (var item in items)
					{
						var p = item.Measure(double.PositiveInfinity, double.PositiveInfinity);
						
					}
				}
				

				// var titleBarHeight = window.TitleBar.HeightRequest == -1 ? cVCView.SafeAreaLayoutGuide.Top :  window.TitleBar.HeightRequest;

				// Set constraints for the custom title bar
				mauiTitleBar.TranslatesAutoresizingMaskIntoConstraints = false;
				NSLayoutConstraint.ActivateConstraints(new[]
				{
					mauiTitleBar.TopAnchor.ConstraintEqualTo(cVCView.TopAnchor),
					mauiTitleBar.LeadingAnchor.ConstraintEqualTo(cVCView.LeadingAnchor),
					mauiTitleBar.TrailingAnchor.ConstraintEqualTo(cVCView.TrailingAnchor),
					mauiTitleBar.HeightAnchor.ConstraintEqualTo(200) // Set the desired height
				});

				// Set constraints for the native content
				nativeContent.View.TranslatesAutoresizingMaskIntoConstraints = false;
				NSLayoutConstraint.ActivateConstraints(new[]
				{
					nativeContent.View.TopAnchor.ConstraintEqualTo(mauiTitleBar.BottomAnchor), // Adjust the top anchor to leave space for the custom title bar
					nativeContent.View.LeadingAnchor.ConstraintEqualTo(cVCView.LeadingAnchor),
					nativeContent.View.TrailingAnchor.ConstraintEqualTo(cVCView.TrailingAnchor),
					nativeContent.View.BottomAnchor.ConstraintEqualTo(cVCView.BottomAnchor)
				});
			}

			handler.PlatformView.RootViewController = containerViewController;
#else

			handler.PlatformView.RootViewController = nativeContent;
#endif

			if (window.VisualDiagnosticsOverlay != null)
				window.VisualDiagnosticsOverlay.Initialize();
		}

		public static void MapX(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateX(view);

		public static void MapY(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateY(view);

		public static void MapWidth(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateWidth(view);

		public static void MapHeight(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateHeight(view);

		public static void MapMaximumWidth(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateMaximumWidth(view);

		public static void MapMaximumHeight(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateMaximumHeight(view);

		public static void MapMinimumWidth(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateMinimumWidth(view);

		public static void MapMinimumHeight(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateMinimumHeight(view);

		public static void MapMenuBar(IWindowHandler handler, IWindow view)
		{
			if (!(OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13)))
				return;

			if (MauiUIApplicationDelegate.Current != null &&
				view is IMenuBarElement mb)
			{
				if (MauiUIApplicationDelegate.Current.MenuBuilder == null)
				{
					UIMenuSystem
						.MainSystem
						.SetNeedsRebuild();
				}
				else
				{
					// The handlers that are part of MenuBar
					// are only relevant while the menu is being built
					// because you can only build a menu while the
					// `AppDelegate.BuildMenu` override is running
					mb.MenuBar?.Handler?.DisconnectHandler();
					mb.MenuBar?
						.ToHandler(handler.MauiContext!)?
						.DisconnectHandler();
				}
			}
		}

		public static void MapRequestDisplayDensity(IWindowHandler handler, IWindow window, object? args)
		{
			if (args is DisplayDensityRequest request)
				request.SetResult(handler.PlatformView.GetDisplayDensity());
		}

		void UpdateVirtualViewFrame(UIWindow window)
		{
			VirtualView.FrameChanged(window.Bounds.ToRectangle());
		}
	}
}