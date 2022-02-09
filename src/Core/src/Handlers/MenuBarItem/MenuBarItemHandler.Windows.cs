using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarItemHandler : ElementHandler<IMenuBarItem, MenuBarItem>, IMenuBarItemHandler
	{

		public static CommandMapper<IMenuBarItem, IMenuBarItemHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IMenuBarItemHandler.Add)] = MapAdd,
			[nameof(IMenuBarItemHandler.Remove)] = MapRemove,
			[nameof(IMenuBarItemHandler.Clear)] = MapClear,
			[nameof(IMenuBarItemHandler.Insert)] = MapInsert,
		};

		public MenuBarItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
		{
		}

		protected override MenuBarItem CreateNativeElement()
		{
			return new MenuBarItem();
		}


		public void Add(IMenuFlyoutItemBase view)
		{
			NativeView.Items.Add(view.ToPlatform(MauiContext));
		}

		public void Remove(IMenuFlyoutItemBase view)
		{
			if (view.Handler != null)
				NativeView.Items.Remove(view.ToPlatform());
		}

		public void Clear()
		{
			NativeView.Items.Clear();
		}

		public void Insert(int index, IMenuFlyoutItemBase view)
		{
			NativeView.Items.Insert(index, view.ToPlatform(MauiContext));
		}
	}
}
