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
			if (VirtualView is not null)
			{
				foreach (var item in VirtualView)
				{
					item.Handler?.DisconnectHandler();
				}
			}

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

		/// <summary>
		///	Maps keyboard accelerator to the Windows platform control.
		/// </summary>
		/// <param name="handler">The handler, of type IMenuFlyoutSubItemHandler.</param>
		/// <param name="view">The view, of type IMenuFlyoutSubItem.</param>
		public static void MapKeyboardAccelerators(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			handler.PlatformView.UpdateKeyboardAccelerators(view);
		}

		public static void MapIsEnabled(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view) =>
			handler.PlatformView.UpdateIsEnabled(view.IsEnabled);

		public static void MapSource(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			var iconSource = view.Source?.ToIconSource(handler.MauiContext!);

			if (iconSource is BitmapIconSource bitmapIconSource)
			{
				bitmapIconSource.ShowAsMonochrome = false;
			}

			handler.PlatformView.Icon = iconSource?.CreateIconElement();
		}

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			PlatformView.Items.Clear();

			foreach (var item in ((IMenuFlyoutSubItem)view))
			{
				PlatformView.Items.Add((MenuFlyoutItemBase)item.ToPlatform(MauiContext!));
			}
		}

		// workaround WinUI bug https://github.com/microsoft/microsoft-ui-xaml/issues/7797
		void Reset()
		{
			if (VirtualView.Parent?.Handler is IElementHandler pvh)
			{
				var vv = VirtualView;
				IList<MenuFlyoutItemBase>? items = null;
				if (pvh.PlatformView is MenuFlyout mf)
				{
					items = mf.Items;

				}
				else if (pvh.PlatformView is MenuFlyoutSubItem mfsi)
				{
					items = mfsi.Items;
				}
				else if (pvh.PlatformView is MenuBarItem menuBarItem)
				{
					items = menuBarItem.Items;
				}

				if (items is not null)
				{
					var index = items.IndexOf(PlatformView);

					if (index < 0)
						return;

					items.RemoveAt(index);

					// You have to create the entire menu whenever adding/removing items.
					// I tried to just remove and re-add this one item from the parent but that doesn't work.
					// Also, if you try to reuse any MenuFlyoutItems on the new control it crashes.
					//
					// The following code crashes with a catastrophic failure.
					//
					// oldMenu.Items.Remove(item);					
					// newMenu.Items.Add(item); 
					// You have to create a new MenuFlyoutSubMenuItem and all of the children

					vv.Handler?.DisconnectHandler();
					items.Insert(index, (MenuFlyoutItemBase)vv.ToPlatform(MauiContext!));
				}
			}
		}

		public void Add(IMenuElement view) =>
			Reset();

		public void Remove(IMenuElement view) =>
			Reset();

		public void Clear() =>
			Reset();

		public void Insert(int index, IMenuElement view) =>
			Reset();
	}
}
