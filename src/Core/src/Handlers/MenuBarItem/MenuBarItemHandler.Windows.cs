using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, MenuBarItem>, IMenuBarItemHandler
	{
		public MenuBarItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
		{
		}

		protected override MenuBarItem CreateNativeElement()
		{
			return new MenuBarItem();
		}

		public static void MapText(IMenuBarItemHandler handler, IMenuBarItem view)
		{
			handler.NativeView.Title = view.Text;
		}

		public static void MapIsEnabled(IMenuBarItemHandler handler, IMenuBarItem view) =>
			handler.NativeView.UpdateIsEnabled(view.IsEnabled);

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			Clear();

			foreach (var item in ((IMenuBarItem)view))
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
