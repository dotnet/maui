using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="Type[@FullName='Microsoft.Maui.Controls.Menu']/Docs" />
	public class Menu : BaseMenuItem, IList<Menu>
	{
		readonly ObservableCollection<Menu> _menus = new ObservableCollection<Menu>();

		readonly ObservableCollection<MenuItem> _items = new ObservableCollection<MenuItem>();

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public Menu()
		{
			_items.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Items));
		}

		public Menu this[int index]
		{
			get { return _menus[index]; }
			set { _menus[index] = value; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Text']/Docs" />
		public string Text
		{
			get;
			set;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Invalidate']/Docs" />
		public void Invalidate() => OnPropertyChanged();

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Items']/Docs" />
		public ObservableCollection<MenuItem> Items => _items;

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Count']/Docs" />
		public int Count => _menus.Count;

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='IsReadOnly']/Docs" />
		public bool IsReadOnly => false;

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Add']/Docs" />
		public void Add(Menu item)
		{
			_menus.Add(item);
			Invalidate();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Clear']/Docs" />
		public void Clear()
		{
			_menus.Clear();
			Invalidate();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Contains']/Docs" />
		public bool Contains(Menu item)
		{
			return _menus.Contains(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='CopyTo']/Docs" />
		public void CopyTo(Menu[] array, int arrayIndex)
		{
			_menus.CopyTo(array, arrayIndex);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='GetEnumerator']/Docs" />
		public IEnumerator<Menu> GetEnumerator()
		{
			return _menus.GetEnumerator();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='IndexOf']/Docs" />
		public int IndexOf(Menu item)
		{
			return _menus.IndexOf(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Insert']/Docs" />
		public void Insert(int index, Menu item)
		{
			_menus.Insert(index, item);
			Invalidate();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Remove']/Docs" />
		public bool Remove(Menu item)
		{
			var result = _menus.Remove(item);
			Invalidate();
			return result;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='RemoveAt']/Docs" />
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
