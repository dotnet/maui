using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Graphics;

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
			set { _menus[index] = value; }
		}

		public void Invalidate() => OnPropertyChanged();

		public int Count => _menus.Count;

		public bool IsReadOnly => false;

		public void Add(IMenuBarItem item)
		{
			_menus.Add(item);
			Invalidate();
		}

		public void Clear()
		{
			_menus.Clear();
			Invalidate();
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
			Invalidate();
		}

		public bool Remove(IMenuBarItem item)
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