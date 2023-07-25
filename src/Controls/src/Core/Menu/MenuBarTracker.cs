#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	internal class MenuBarTracker : MenuItemTracker<MenuBarItem>
	{
		MenuBarItemComparer _Comparer;
		MenuBar _menuBar;
		Element _parent;
		string _handlerProperty;

		public MenuBarTracker() : this(null, null)
		{
		}

		public MenuBarTracker(Element parent, string handlerProperty)
		{
			_menuBar = new MenuBar();
			_parent = parent;
			_handlerProperty = handlerProperty;
			CollectionChanged += OnMenuBarItemCollectionChanged;
		}

		void OnMenuBarItemCollectionChanged(object sender, EventArgs e)
		{
			_menuBar.SyncMenuBarItemsFromPages(ToolbarItems);

			if (_menuBar.Count == 0)
			{
				_menuBar.Parent?.RemoveLogicalChild(_menuBar);
			}
			else if (_menuBar.Parent != _parent)
			{
				_parent.AddLogicalChild(_menuBar);
			}

			if (_handlerProperty != null)
			{
				_parent?.Handler?.UpdateValue(_handlerProperty);
			}
		}

		public MenuBar MenuBar
		{
			get
			{
				var menuBarItems = ToolbarItems;
				if (menuBarItems.Count == 0)
					return null;

				_menuBar.SyncMenuBarItemsFromPages(ToolbarItems);

				return _menuBar;
			}
		}


		protected override IList<MenuBarItem> GetMenuItems(Page page) =>
			page.MenuBarItems;

		protected override IComparer<MenuBarItem> CreateComparer() =>
			_Comparer ??= new MenuBarItemComparer();

		class MenuBarItemComparer : IComparer<MenuBarItem>
		{
			public int Compare(MenuBarItem x, MenuBarItem y) => x.Priority.CompareTo(y.Priority);
		}
	}
}