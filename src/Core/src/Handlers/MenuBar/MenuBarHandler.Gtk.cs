using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Handlers
{
	//https://docs.gtk.org/gtk3/class.MenuShell.html
	public abstract class GtkMenuShellHandler<TVirtualView, TPlatform, TVirtualItem, TPlatformItem> : ElementHandler<TVirtualView, TPlatform>
		where TPlatform : Gtk.MenuShell, new()
		where TVirtualView : class, IList<TVirtualItem>, IElement
		where TVirtualItem : IElement
		where TPlatformItem : Gtk.MenuItem
	{
		protected GtkMenuShellHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper) { }

		protected override TPlatform CreatePlatformElement()
		{
			return new();
		}

		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);
			Clear();

			foreach (var item in (TVirtualView)view)
			{
				Add(item);
			}
		}

		public void Add(TVirtualItem view)
		{
			var platformItem = (TPlatformItem)view.ToPlatform(MauiContext!);
			PlatformView.Append(platformItem);
			platformItem.Show();
		}

		public void Remove(TVirtualItem view)
		{
			var platformItem = (TPlatformItem)view.ToPlatform(MauiContext!);
			PlatformView.Remove(platformItem);
		}


		public void Insert(int index, TVirtualItem view)
		{
			var platformItem = (TPlatformItem)view.ToPlatform(MauiContext!);
			PlatformView.Insert(platformItem, index);
			platformItem.Show();
		}

		public void Clear()
		{
			foreach (var c in PlatformView.Children.ToArray())
			{
				PlatformView.Remove(c);
			}
		}
	}

	public partial class MenuBarHandler : GtkMenuShellHandler<IMenuBar, MauiMenuBar, IMenuBarItem, Gtk.MenuItem> { }
}