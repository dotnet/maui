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
			_menuBar.Parent = parent;
			_parent = parent;
			_handlerProperty = handlerProperty;
			CollectionChanged += OnMenuBarItemCollectionChanged;
		}

		void OnMenuBarItemCollectionChanged(object sender, EventArgs e)
		{
			if (_handlerProperty != null)
			{
				// For now we just reset the entire menu if users modify the menubar
				// collection
				//if (_parent is IMenuBarElement mbe &&
				//	mbe.MenuBar?.Handler != null)
				//{
				//	mbe.MenuBar.Handler.DisconnectHandler();
				//}

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

				_menuBar.ReplaceWith(ToolbarItems);

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