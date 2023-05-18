#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	internal class ToolbarTracker : MenuItemTracker<ToolbarItem>
	{
		ToolBarItemComparer _toolBarItemComparer;
		protected override IList<ToolbarItem> GetMenuItems(Page page) =>
			page.ToolbarItems;

		protected override IComparer<ToolbarItem> CreateComparer() =>
			_toolBarItemComparer ??= new ToolBarItemComparer();

		class ToolBarItemComparer : IComparer<ToolbarItem>
		{
			public int Compare(ToolbarItem x, ToolbarItem y) => x.Priority.CompareTo(y.Priority);
		}
	}
}