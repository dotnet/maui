using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	public partial class MenuBarItem : BaseMenuItem, IMenuBarItem
	{
		public static readonly BindableProperty TextProperty =
			BindableProperty.Create(nameof(Text), typeof(string), typeof(MenuBarItem), null);

		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(nameof(IsEnabled), typeof(bool),
			typeof(MenuBarItem), true);

		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}


		ReadOnlyCastingList<Element, IMenuElement> _logicalChildren;
		readonly ObservableCollection<IMenuElement> _menus = new ObservableCollection<IMenuElement>();

		internal override IReadOnlyList<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCastingList<Element, IMenuElement>(_menus);

		public IMenuElement this[int index]
		{
			get { return _menus[index]; }
			set { _menus[index] = value; }
		}

		public void Invalidate() => OnPropertyChanged();

		public int Count => _menus.Count;

		public bool IsReadOnly => false;

		public void Add(IMenuElement item)
		{
			_menus.Add(item);
			Invalidate();
		}

		public void Clear()
		{
			_menus.Clear();
			Invalidate();
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
			Invalidate();
		}

		public bool Remove(IMenuElement item)
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