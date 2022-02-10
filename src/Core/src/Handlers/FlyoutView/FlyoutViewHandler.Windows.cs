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
		protected override RootNavigationView CreateNativeView()
		{
			var navigationView = new RootNavigationView();

			return navigationView;
		}

		protected override void ConnectHandler(RootNavigationView nativeView)
		{
			_navigationRootManager = MauiContext?.GetNavigationRootManager();
			nativeView.PaneOpened += OnPaneOepened;
		}

		protected override void DisconnectHandler(RootNavigationView nativeView)
		{
			nativeView.PaneOpened -= OnPaneOepened;
		}

		void OnPaneOepened(NavigationView sender, object args)
		{
			VirtualView.IsPresented = sender.IsPaneOpen;
		}

		void UpdateDetail()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Detail.ToPlatform(MauiContext);

			NativeView.Content = VirtualView.Detail.ToPlatform();
		}

		void UpdateFlyout()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Flyout.ToPlatform(MauiContext);

			NativeView.ReplacePaneMenuItemsWithCustomContent(VirtualView.Flyout);
		}

		public static void MapDetail(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateDetail();
		}

		public static void MapFlyout(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateFlyout();
		}

		public static void MapIsPresented(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.NativeView.IsPaneOpen = flyoutView.IsPresented;
		}

		public static void MapFlyoutWidth(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (flyoutView.Width >= 0)
				handler.NativeView.OpenPaneLength = flyoutView.Width;
			else
				handler.NativeView.OpenPaneLength = 320;
			// At some point this Template Setting is going to show up with a bump to winui
			//handler.NativeView.OpenPaneLength = handler.NativeView.TemplateSettings.OpenPaneWidth;

		}

		public static void MapFlyoutBehavior(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.NativeView.UpdateFlyoutBehavior(flyoutView);
		}

		public static void MapIsGestureEnabled(FlyoutViewHandler handler, IFlyoutView view)
		{
		}
	}
}
