using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms
{
	public class Menu : BaseMenuItem, IList<Menu>
	{
		readonly ObservableCollection<Menu> _menus = new ObservableCollection<Menu>();

		readonly ObservableCollection<MenuItem> _items = new ObservableCollection<MenuItem>();

		public Menu()
		{
			_items.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Items));
		}

		public Menu this[int index]
		{
			get { return _menus[index]; }
			set { _menus[index] = value; }
		}

		public string Text
		{
			get;
			set;
		}

		public void Invalidate() => OnPropertyChanged();

		public ObservableCollection<MenuItem> Items => _items;

		public int Count => _menus.Count;

		public bool IsReadOnly => false;

		public void Add(Menu item)
		{
			_menus.Add(item);
			Invalidate();
		}

		public void Clear()
		{
			_menus.Clear();
			Invalidate();
		}

		public bool Contains(Menu item)
		{
			return _menus.Contains(item);
		}

		public void CopyTo(Menu[] array, int arrayIndex)
		{
			_menus.CopyTo(array, arrayIndex);
		}

		public IEnumerator<Menu> GetEnumerator()
		{
			return _menus.GetEnumerator();
		}

		public int IndexOf(Menu item)
		{
			return _menus.IndexOf(item);
		}

		public void Insert(int index, Menu item)
		{
			_menus.Insert(index, item);
			Invalidate();
		}

		public bool Remove(Menu item)
		{
			var result = _menus.Remove(item);
			Invalidate();
			return result;
		}

		public void RemoveAt(int index)
		{
			_menus.RemoveAt(index);
			Invalidate();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _menus.GetEnumerator();
		}
	}
}