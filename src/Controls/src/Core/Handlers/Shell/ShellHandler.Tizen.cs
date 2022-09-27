#nullable enable

using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class ShellHandler : ViewHandler<Shell, ShellView>
	{
		protected override ShellView CreatePlatformView()
		{
			var shellView = new ShellView();
			shellView.SetElement(VirtualView, MauiContext!);
			return shellView;
		}

		protected override void ConnectHandler(ShellView platformView)
		{
			platformView.Toggled += OnToggled;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(ShellView platformView)
		{
			platformView.Toggled -= OnToggled;
			base.DisconnectHandler(platformView);
		}

		public static void MapFlyout(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyout(flyoutView.Flyout);
		}

		public static void MapIsPresented(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.IsOpened = flyoutView.IsPresented;
		}

		public static void MapFlyoutBehavior(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateFlyoutBehavior(flyoutView.FlyoutBehavior);
		}

		public static void MapFlyoutWidth(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateDrawerWidth(flyoutView.FlyoutWidth);
		}

		public static void MapFlyoutBackground(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateBackgroundColor(view.BackgroundColor);
		}

		public static void MapCurrentItem(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateCurrentItem(view.CurrentItem);
		}

		public static void MapFlyoutBackdrop(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateFlyoutBackDrop(view.FlyoutBackdrop);
		}

		public static void MapFlyoutFooter(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateFlyoutFooter(view);
		}

		public static void MapFlyoutHeader(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateFlyoutHeader(view);
		}

		public static void MapFlyoutHeaderBehavior(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateFlyoutHeader(view);
		}

		public static void MapItems(ShellHandler handler, IFlyoutView flyoutView)
		{
			handler.PlatformView.UpdateItems();
		}

		public static void MapFlyoutItems(ShellHandler handler, Shell view)
		{
			handler.PlatformView.UpdateItems();
		}

		void OnToggled(object? sender, EventArgs e)
		{
			if (sender is ShellView shellView)
				VirtualView.FlyoutIsPresented = shellView.IsOpened;
		}

	}
}
