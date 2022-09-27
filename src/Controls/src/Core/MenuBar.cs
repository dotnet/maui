using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	public partial class MenuBar : Element, IMenuBar
	{
		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(nameof(IsEnabled), typeof(bool),
			typeof(MenuBar), true);

		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		ReadOnlyCastingList<Element, IMenuBarItem> _logicalChildren;
		readonly ObservableCollection<IMenuBarItem> _menus = new ObservableCollection<IMenuBarItem>();

		internal override IReadOnlyList<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCastingList<Element, IMenuBarItem>(_menus);

		public IMenuBarItem this[int index]
		{
			get { return _menus[index]; }
			set
			{
				RemoveAt(index);
				Insert(index, value);
			}
		}

		public int Count => _menus.Count;

		public bool IsReadOnly => false;

		public void Add(IMenuBarItem item)
		{
			var index = _menus.Count;
			_menus.Add(item);
			NotifyHandler(nameof(IMenuBarHandler.Add), index, item);

			// Take care of the Element internal bookkeeping
			if (item is Element element)
			{
				OnChildAdded(element);
			}
		}

		internal void ReplaceWith(IList<MenuBarItem> menuBarItems)
		{
			Clear();
			foreach (var menuItem in menuBarItems)
				Add(menuItem);
		}

		public void Clear()
		{
			for (int i = _menus.Count - 1; i >= 0; i--)
				RemoveAt(i);
		}

		public bool Contains(IMenuBarItem item)
		{
			return _menus.Contains(item);
		}

		public void CopyTo(IMenuBarItem[] array, int arrayIndex)
		{
			_menus.CopyTo(array, arrayIndex);
		}

		public IEnumerator<IMenuBarItem> GetEnumerator()
		{
			return _menus.GetEnumerator();
		}

		public int IndexOf(IMenuBarItem item)
		{
			return _menus.IndexOf(item);
		}

		public void Insert(int index, IMenuBarItem item)
		{
			_menus.Insert(index, item);
			NotifyHandler(nameof(IMenuBarHandler.Insert), index, item);

			// Take care of the Element internal bookkeeping
			if (item is Element element)
			{
				OnChildAdded(element);
			}
		}

		public bool Remove(IMenuBarItem item)
		{
			var index = _menus.IndexOf(item);
			var result = _menus.Remove(item);
			NotifyHandler(nameof(IMenuBarHandler.Remove), index, item);

			// Take care of the Element internal bookkeeping
			if (item is Element element)
			{
				OnChildRemoved(element, index);
			}

			return result;
		}

		public void RemoveAt(int index)
		{
			var item = _menus[index];

			if (index < 0)
				return;

			_menus.RemoveAt(index);
			NotifyHandler(nameof(IMenuBarHandler.Remove), index, item);

			// Take care of the Element internal bookkeeping
			if (item is Element element)
			{
				OnChildRemoved(element, index);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _menus.GetEnumerator();
		}

		void NotifyHandler(string action, int index, IMenuBarItem view)
		{
			Handler?.Invoke(action, new Maui.Handlers.MenuBarHandlerUpdate(index, view));
		}
	}
}