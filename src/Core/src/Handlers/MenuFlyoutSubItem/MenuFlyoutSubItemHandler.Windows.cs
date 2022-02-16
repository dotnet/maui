using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSubItemHandler
	{
		protected override MenuFlyoutSubItem CreateNativeElement()
		{
			return new MenuFlyoutSubItem();
		}

		protected override void DisconnectHandler(MenuFlyoutSubItem nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.Tapped -= OnTapped;
		}

		protected override void ConnectHandler(MenuFlyoutSubItem nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.Tapped += OnTapped;
		}

		void OnTapped(object sender, UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			VirtualView.Clicked();
		}

		public static void MapText(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler.NativeView.Text = view.Text;
		}

		public static void MapIsEnabled(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view) =>
			handler.NativeView.UpdateIsEnabled(view.IsEnabled);

		public static void MapSource(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler.NativeView.Icon =
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
			NativeView.Items.Add((MenuFlyoutItemBase)view.ToPlatform(MauiContext!));
		}

		public void Remove(IMenuElement view)
		{
			if (view.Handler != null)
				NativeView.Items.Remove((MenuFlyoutItemBase)view.ToPlatform());
		}

		public void Clear()
		{
			NativeView.Items.Clear();
		}

		public void Insert(int index, IMenuElement view)
		{
			NativeView.Items.Insert(index, (MenuFlyoutItemBase)view.ToPlatform(MauiContext!));
		}
	}
}
