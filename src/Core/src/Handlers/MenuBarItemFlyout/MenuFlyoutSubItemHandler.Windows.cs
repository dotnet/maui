using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuFlyoutSubItemHandler : 
		MenuFlyoutItemBaseHandler<IMenuFlyoutSubItem, MenuFlyoutSubItem>
	{
		protected override MenuFlyoutSubItem CreateNativeElement()
		{
			return new MenuFlyoutSubItem();
		}

		public static void MapText(IMenuFlyoutSubItemHandler handler, IMenuFlyoutSubItem view)
		{
			// TODO MAUI Fix the types on interfaces
			((MenuFlyoutSubItem)handler.NativeView!).Text = view.Text;
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

		public void Add(IMenuFlyoutItemBase view)
		{
			NativeView.Items.Add((MenuFlyoutItemBase)view.ToPlatform(MauiContext!));
		}

		public void Remove(IMenuFlyoutItemBase view)
		{
			if (view.Handler != null)
				NativeView.Items.Remove((MenuFlyoutItemBase)view.ToPlatform());
		}

		public void Clear()
		{
			NativeView.Items.Clear();
		}

		public void Insert(int index, IMenuFlyoutItemBase view)
		{
			NativeView.Items.Insert(index, (MenuFlyoutItemBase)view.ToPlatform(MauiContext!));
		}
	}
}
