#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{

	public class MenuFlyout : FlyoutBase, IMenuFlyout // Same pattern as MenuBarItem
	{
		readonly List<IMenuElement> _menus = new List<IMenuElement>();

		private protected override IList<Element> LogicalChildrenInternalBackingStore
			=> new CastingList<Element, IMenuElement>(_menus);

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
			AddLogicalChild((Element)item);
			NotifyHandler(nameof(IMenuFlyoutHandler.Add), index, item);

			// Take care of the Element internal bookkeeping
			if (item is Element element)
			{
				OnChildAdded(element);
			}
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
			InsertLogicalChild(index, (Element)item);
			NotifyHandler(nameof(IMenuFlyoutHandler.Insert), index, item);
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
			NotifyHandler(nameof(IMenuFlyoutHandler.Remove), index, item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _menus.GetEnumerator();
		}

		void NotifyHandler(string action, int index, IMenuElement view)
		{
			Handler?.Invoke(action, new Maui.Handlers.ContextFlyoutItemHandlerUpdate(index, view));
		}
	}
}
