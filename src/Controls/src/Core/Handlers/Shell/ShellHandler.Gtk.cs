using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{

	public partial class ShellHandler : ViewHandler<Shell, ShellView>
	{

		protected override ShellView CreatePlatformView()
		{
			return new();
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

	}

}