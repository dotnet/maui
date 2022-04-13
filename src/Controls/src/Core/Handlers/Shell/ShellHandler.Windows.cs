using Microsoft.Maui.Controls.Platform;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellView>
	{
		protected override ShellView CreatePlatformView()
		{
			var shellView = new ShellView();
			shellView.SetElement(VirtualView);
			return shellView;
		}

		protected override void ConnectHandler(ShellView platformView)
		{
			base.ConnectHandler(platformView);
			platformView.PaneOpened += OnPaneOpened;
			platformView.PaneOpening += OnPaneOpening;
			platformView.PaneClosing += OnPaneClosing;
			platformView.ItemInvoked += OnMenuItemInvoked;
		}

		void OnMenuItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
		{
			var item = args.InvokedItemContainer?.DataContext as Element;
			if (item != null)
				(VirtualView as IShellController)?.OnFlyoutItemSelected(item);
		}

		void OnPaneOpened(UI.Xaml.Controls.NavigationView sender, object args)
		{
			PlatformView.UpdateFlyoutBackdrop();
		}

		void OnPaneClosing(UI.Xaml.Controls.NavigationView sender, UI.Xaml.Controls.NavigationViewPaneClosingEventArgs args)
		{
			args.Cancel = true;
			VirtualView.FlyoutIsPresented = false;
		}

		void OnPaneOpening(UI.Xaml.Controls.NavigationView sender, object args)
		{
			UpdateValue(nameof(Shell.FlyoutBackground));
			UpdateValue(nameof(Shell.FlyoutVerticalScrollMode));
			PlatformView.UpdateFlyoutBackdrop();
			PlatformView.UpdateFlyoutPosition();
			VirtualView.FlyoutIsPresented = true;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (PlatformView.Element != view)
				PlatformView.SetElement((Shell)view);
		}

		public static void MapFlyoutBackdrop(ShellHandler handler, Shell view)
		{
			if (Brush.IsNullOrEmpty(view.FlyoutBackdrop))
				handler.PlatformView.FlyoutBackdrop = null;
			else
				handler.PlatformView.FlyoutBackdrop = view.FlyoutBackdrop;
		}

		public static void MapCurrentItem(ShellHandler handler, Shell view)
		{
			handler.PlatformView.SwitchShellItem(view.CurrentItem, true);
		}

		public static void MapFlyoutBackground(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdatePaneBackground(
				!Brush.IsNullOrEmpty(view.FlyoutBackground) ?
					view.FlyoutBackground :
					view.FlyoutBackgroundColor?.AsPaint());
		}

		public static void MapFlyoutVerticalScrollMode(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateFlyoutVerticalScrollMode((WScrollMode)(int)view.FlyoutVerticalScrollMode);
		}

		public static void MapFlyout(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.FlyoutCustomContent = flyoutView.Flyout?.ToPlatform(handler.MauiContext);
		}

		public static void MapIsPresented(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.IsPaneOpen = flyoutView.IsPresented;
		}

		public static void MapFlyoutWidth(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyoutWidth(flyoutView);
		}

		public static void MapFlyoutBehavior(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyoutBehavior(flyoutView);
		}

		public static void MapFlyoutFooter(ShellHandler handler, Shell view)
		{
			if (handler.PlatformView.PaneFooter == null)
				handler.PlatformView.PaneFooter = new ShellFooterView(view);
		}

		public static void MapFlyoutHeader(ShellHandler handler, Shell view)
		{
			if (handler.PlatformView.PaneHeader == null)
				handler.PlatformView.PaneHeader = new ShellHeaderView(view);
		}

		public static void MapItems(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateMenuItemSource();
		}

		public static void MapFlyoutItems(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateMenuItemSource();
		}
	}
}
