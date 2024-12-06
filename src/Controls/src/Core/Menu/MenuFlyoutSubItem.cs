#nullable disable
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	public partial class MenuFlyoutSubItem : MenuFlyoutItem, IMenuFlyoutSubItem
	{
		readonly List<IElement> _menus = new List<IElement>();

		public MenuFlyoutSubItem()
		{
			LogicalChildrenInternalBackingStore = new CastingList<Element, IElement>(_menus);
		}

		private protected override IList<Element> LogicalChildrenInternalBackingStore { get; }

		public IMenuElement this[int index]
		{
			get { return _menus[index] as IMenuElement; }
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
			return _menus.Cast<IMenuElement>().GetEnumerator();
		}

		public int IndexOf(IMenuElement item)
		{
			return _menus.IndexOf(item);
		}

		public void Insert(int index, IMenuElement item)
		{
			InsertLogicalChild(index, (Element)item);
			NotifyHandler(nameof(IMenuFlyoutSubItemHandler.Insert), index, item);
		}

		public bool Remove(IMenuElement item)
		{
			var index = _menus.IndexOf(item);
			var result = RemoveLogicalChild((Element)item, index);
			NotifyHandler(nameof(IMenuFlyoutHandler.Remove), index, item);

			return result;
		}

		public void RemoveAt(int index)
		{
			var item = _menus[index];
			RemoveLogicalChild((Element)item, index);
			NotifyHandler(nameof(IMenuFlyoutHandler.Remove), index, item as IMenuElement);
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