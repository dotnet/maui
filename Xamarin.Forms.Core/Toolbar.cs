using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	// Marked as internal for 1.0 release until we are ready to release this
	[RenderWith(typeof(_ToolbarRenderer))]
	internal class Toolbar : View
	{
		readonly List<ToolbarItem> _items = new List<ToolbarItem>();

		public ReadOnlyCollection<ToolbarItem> Items
		{
			get { return new ReadOnlyCollection<ToolbarItem>(_items); }
		}

		public void Add(ToolbarItem item)
		{
			if (_items.Contains(item))
				return;
			_items.Add(item);
			if (ItemAdded != null)
				ItemAdded(this, new ToolbarItemEventArgs(item));
		}

		public void Clear()
		{
			_items.Clear();
		}

		public event EventHandler<ToolbarItemEventArgs> ItemAdded;

		public event EventHandler<ToolbarItemEventArgs> ItemRemoved;

		public void Remove(ToolbarItem item)
		{
			if (_items.Remove(item))
			{
				if (ItemRemoved != null)
					ItemRemoved(this, new ToolbarItemEventArgs(item));
			}
		}
	}
}