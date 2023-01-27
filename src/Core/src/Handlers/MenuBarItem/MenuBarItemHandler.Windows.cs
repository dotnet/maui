using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, MenuBarItem>, IMenuBarItemHandler
	{
		protected override MenuBarItem CreatePlatformElement()
		{
			return new MenuBarItem();
		}

		public static void MapText(IMenuBarItemHandler handler, IMenuBarItem view)
		{
			handler.PlatformView.Title = view.Text;
		}

		public static void MapIsEnabled(IMenuBarItemHandler handler, IMenuBarItem view) =>
			handler.PlatformView.UpdateIsEnabled(view.IsEnabled);

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			Clear();

			foreach (var item in ((IMenuBarItem)view))
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
