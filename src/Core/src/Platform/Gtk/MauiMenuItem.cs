using System.Linq;
using Gtk;

namespace Microsoft.Maui.Platform;

// https://docs.gtk.org/gtk3/class.MenuItem.html

public class MauiMenuItem : MenuItem
{
	Menu EnsureSubMenu => (Menu)(Submenu ??= new Menu());

	public void AppendSubItem(MenuItem subItem)
	{
		EnsureSubMenu.Append(subItem);
	}

	public void RemoveSubItem(MenuItem subItem)
	{
		if (Submenu is not { })
			return;
		EnsureSubMenu.Remove(subItem);
	}

	public void InsertSubItem(MenuItem subItem, int index)
	{
		EnsureSubMenu.Insert(subItem, index);
	}

	public void ClearSubItems()
	{
		if (Submenu is not { })
			return;
		
		foreach (var m in EnsureSubMenu.Children.ToArray())
		{
			EnsureSubMenu.Remove(m);
		}
	}


}