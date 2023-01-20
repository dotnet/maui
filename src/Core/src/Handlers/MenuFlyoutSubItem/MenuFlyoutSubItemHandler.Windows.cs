using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSubItemHandler
	{
		protected override MenuFlyoutSubItem CreatePlatformElement()
		{
			return new MenuFlyoutSubItem();
		}

		protected override void DisconnectHandler(MenuFlyoutSubItem PlatformView)
		{
			base.DisconnectHandler(PlatformView);
			PlatformView.Tapped -= OnTapped;
		}

		protected override void ConnectHandler(MenuFlyoutSubItem PlatformView)
		{
			base.ConnectHandler(PlatformView);
			PlatformView.Tapped += OnTapped;
		}

		void OnTapped(object sender, UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			VirtualView.Clicked();
		}

		public static void MapText(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler.PlatformView.Text = view.Text;
		}

		public static void MapIsEnabled(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view) =>
			handler.PlatformView.UpdateIsEnabled(view.IsEnabled);

		public static void MapSource(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler.PlatformView.Icon =
				view.Source?.ToIconSource(handler.MauiContext!)?.CreateIconElement();
		}

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			Clear();

			foreach (var item in ((IMenuFlyoutSubItem)view))
			{
				Add(item);
			}
		}

		public void Add(IMenuElement view)
		{
			PlatformView.Items.Add((MenuFlyoutItemBase)view.ToPlatform(MauiContext!));
		}

		public void Remove(IMenuElement view)
		{
			if (view.Handler != null)
				PlatformView.Items.Remove((MenuFlyoutItemBase)view.ToPlatform());
		}

		public void Clear()
		{
			PlatformView.Items.Clear();
		}

		public void Insert(int index, IMenuElement view)
		{
			PlatformView.Items.Insert(index, (MenuFlyoutItemBase)view.ToPlatform(MauiContext!));
		}
	}
}
