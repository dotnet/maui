using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutItemHandler
	{
		protected override MenuFlyoutItem CreatePlatformElement()
		{
			return new MenuFlyoutItem();
		}

		protected override void ConnectHandler(MenuFlyoutItem PlatformView)
		{
			base.ConnectHandler(PlatformView);
			PlatformView.Click += OnClicked;
		}

		protected override void DisconnectHandler(MenuFlyoutItem PlatformView)
		{
			base.DisconnectHandler(PlatformView);
			PlatformView.Click -= OnClicked;
		}

		void OnClicked(object sender, UI.Xaml.RoutedEventArgs e)
		{
			VirtualView.Clicked();
		}

		public static void MapSource(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
		{
			handler.PlatformView.Icon =
				view.Source?.ToIconSource(handler.MauiContext!)?.CreateIconElement();
		}

		public static void MapText(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view)
		{
			handler.PlatformView.Text = view.Text;
		}

		public static void MapIsEnabled(IMenuFlyoutItemHandler handler, IMenuFlyoutItem view) =>
			handler.PlatformView.UpdateIsEnabled(view.IsEnabled);
	}
}
