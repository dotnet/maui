using Microsoft.Maui.Controls.Platform;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellView>
	{
		protected override ShellView CreateNativeView()
		{
			var shellView = new ShellView();
			shellView.SetElement(VirtualView);
			return shellView;
		}

		protected override void ConnectHandler(ShellView nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.PaneOpened += OnPaneOpened;
			nativeView.PaneOpening += OnPaneOpening;
			nativeView.PaneClosing += OnPaneClosing;
			nativeView.ItemInvoked += OnMenuItemInvoked;
		}

		void OnMenuItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
		{
			var item = args.InvokedItemContainer?.DataContext as Element;
			if (item != null)
				(VirtualView as IShellController)?.OnFlyoutItemSelected(item);
		}

		void OnPaneOpened(UI.Xaml.Controls.NavigationView sender, object args)
		{
			NativeView.UpdateFlyoutBackdrop();
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
			NativeView.UpdateFlyoutBackdrop();
			NativeView.UpdateFlyoutPosition();
			VirtualView.FlyoutIsPresented = true;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (NativeView.Element != view)
				NativeView.SetElement((Shell)view);
		}

		public static void MapFlyoutBackdrop(ShellHandler handler, Shell view)
		{
			if (Brush.IsNullOrEmpty(view.FlyoutBackdrop))
				handler.NativeView.FlyoutBackdrop = null;
			else
				handler.NativeView.FlyoutBackdrop = view.FlyoutBackdrop;
		}

		public static void MapCurrentItem(ShellHandler handler, Shell view)
		{
			handler.NativeView.SwitchShellItem(view.CurrentItem, true);
		}

		public static void MapFlyoutBackground(ShellHandler handler, Shell view)
		{
			handler.NativeView.UpdatePaneBackground(
				!Brush.IsNullOrEmpty(view.FlyoutBackground) ?
					view.FlyoutBackground :
					view.FlyoutBackgroundColor?.AsPaint());
		}

		public static void MapFlyoutVerticalScrollMode(ShellHandler handler, Shell view)
		{
			handler.NativeView.UpdateFlyoutVerticalScrollMode((WScrollMode)(int)view.FlyoutVerticalScrollMode);
		}

		public static void MapFlyout(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.NativeView.ReplacePaneMenuItemsWithCustomContent(flyoutView.Flyout);
		}

		public static void MapIsPresented(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.NativeView.IsPaneOpen = flyoutView.IsPresented;
		}

		public static void MapFlyoutWidth(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.NativeView.UpdateFlyoutWidth(flyoutView);
		}

		public static void MapFlyoutBehavior(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.NativeView.UpdateFlyoutBehavior(flyoutView);
		}

		public static void MapFlyoutFooter(ShellHandler handler, Shell view)
		{
			if (handler.NativeView.PaneFooter == null)
				handler.NativeView.PaneFooter = new ShellFooterView(view);
		}

		public static void MapFlyoutHeader(ShellHandler handler, Shell view)
		{
			if (handler.NativeView.PaneCustomContent == null)
				handler.NativeView.PaneCustomContent = new ShellHeaderView(view);
		}

		public static void MapItems(ShellHandler handler, Shell view)
		{
			handler.NativeView.UpdateMenuItemSource();
		}

		public static void MapFlyoutItems(ShellHandler handler, Shell view)
		{
			handler.NativeView.UpdateMenuItemSource();
		}
	}
}
