#nullable disable
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	[ElementHandler(typeof(MenuFlyoutSubItemHandler))]
	public partial class MenuFlyoutSubItem : MenuFlyoutItem, IMenuFlyoutSubItem
	{
		readonly List<IMenuElement> _menus = new List<IMenuElement>();

		public MenuFlyoutSubItem()
		{
		}

		public IMenuElement this[int index]
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

		public void Add(IMenuElement item)
		{
			var index = _menus.Count;
			_menus.Add(item);
			AddLogicalChild((Element)item);
			NotifyHandler(nameof(IMenuBarItemHandler.Add), index, item);
		}

		public void Clear()
		{
			for (int i = _menus.Count - 1; i >= 0; i--)
				RemoveAt(i);
		}

		public bool Contains(IMenuElement item)
		{
			return _menus.Contains(item);
		}

		public void CopyTo(IMenuElement[] array, int arrayIndex)
		{
			_menus.CopyTo(array, arrayIndex);
		}

		public IEnumerator<IMenuElement> GetEnumerator()
		{
			return _menus.GetEnumerator();
		}

		public int IndexOf(IMenuElement item)
		{
			return _menus.IndexOf(item);
		}

		public void Insert(int index, IMenuElement item)
		{
			_menus.Insert(index, item);
			InsertLogicalChild(index, (Element)item);
			NotifyHandler(nameof(IMenuFlyoutSubItemHandler.Insert), index, item);
		}

		public bool Remove(IMenuElement item)
		{
			var index = _menus.IndexOf(item);
			_menus.RemoveAt(index);

			// Logical Child Index might not match location in _menus list
			var result = RemoveLogicalChild((Element)item);
			NotifyHandler(nameof(IMenuFlyoutHandler.Remove), index, item);

			return result;
		}

		public void RemoveAt(int index)
		{
			var item = _menus[index];
			_menus.RemoveAt(index);

			// Logical Child Index might not match location in _menus list
			RemoveLogicalChild((Element)item);
			NotifyHandler(nameof(IMenuFlyoutHandler.Remove), index, item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _menus.GetEnumerator();
		}

		void NotifyHandler(string action, int index, IMenuElement view)
		{
			Handler?.Invoke(action, new Maui.Handlers.MenuFlyoutSubItemHandlerUpdate(index, view));
		}
	}
}