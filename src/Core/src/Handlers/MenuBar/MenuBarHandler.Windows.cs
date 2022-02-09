using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class MenuBarHandler : ElementHandler<IMenuBar, MenuBar>, IMenuBarHandler
	{

		public static CommandMapper<IMenuBar, IMenuBarHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IMenuBarHandler.Add)] = MapAdd,
			[nameof(IMenuBarHandler.Remove)] = MapRemove,
			[nameof(IMenuBarHandler.Clear)] = MapClear,
			[nameof(IMenuBarHandler.Insert)] = MapInsert,
		};

		public MenuBarHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
		{
		}

		protected override MenuBar CreateNativeElement()
		{
			return new MenuBar();
		}


		public void Add(IMenuBarItem view)
		{
			NativeView.Items.Add(view.ToPlatform(MauiContext));
		}

		public void Remove(IMenuBarItem view)
		{
			if (view.Handler != null)
				NativeView.Items.Remove(view.ToPlatform());
		}

		public void Clear()
		{
			NativeView.Items.Clear();
		}

		public void Insert(int index, IMenuBarItem view)
		{
			NativeView.Items.Insert(index, view.ToPlatform(MauiContext));
		}
	}
}
