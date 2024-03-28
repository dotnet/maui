using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, RootNavigationView>
	{
		NavigationRootManager? _navigationRootManager;
		protected override RootNavigationView CreatePlatformView()
		{
			var navigationView = new RootNavigationView();

			return navigationView;
		}

		protected override void ConnectHandler(RootNavigationView platformView)
		{
			_navigationRootManager = MauiContext?.GetNavigationRootManager();
			platformView.PaneOpened += OnPaneOpened;
		}

		protected override void DisconnectHandler(RootNavigationView platformView)
		{
			platformView.PaneOpened -= OnPaneOpened;
		}

		void OnPaneOpened(NavigationView sender, object args)
		{
			VirtualView.IsPresented = sender.IsPaneOpen;
		}

		static void UpdateDetail(IFlyoutViewHandler handler)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.VirtualView.Detail.ToPlatform(handler.MauiContext);

			handler.PlatformView.Content = handler.VirtualView.Detail.ToPlatform();
		}

		static void UpdateFlyout(IFlyoutViewHandler handler)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.VirtualView.Flyout.ToPlatform(handler.MauiContext);

			if (handler.PlatformView is RootNavigationView rnv)
				rnv.FlyoutView = handler.VirtualView.Flyout;

			MauiNavigationView.FlyoutCustomContent = handler.VirtualView.Flyout?.ToPlatform(handler.MauiContext);
		}

		public static void MapDetail(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			UpdateDetail(handler);
		}

		public static void MapFlyout(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			UpdateFlyout(handler);
		}

		public static void MapIsPresented(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.IsPaneOpen = flyoutView.IsPresented;
		}

		public static void MapFlyoutWidth(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (flyoutView.Width >= 0)
				handler.PlatformView.OpenPaneLength = flyoutView.Width;
			else
				handler.PlatformView.OpenPaneLength = 320;
			// At some point this Template Setting is going to show up with a bump to winui
			//handler.PlatformView.OpenPaneLength = handler.PlatformView.TemplateSettings.OpenPaneWidth;

		}

		public static void MapFlyoutBehavior(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyoutBehavior(flyoutView);
		}

		public static void MapIsGestureEnabled(IFlyoutViewHandler handler, IFlyoutView view)
		{
		}
	}
}
