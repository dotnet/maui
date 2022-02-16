using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="Type[@FullName='Microsoft.Maui.Controls.Menu']/Docs" />
	public abstract class Menu<TMenuType> : BaseMenuItem, IList<TMenuType>
		where TMenuType : class
	{
		ReadOnlyCastingList<Element, TMenuType> _logicalChildren;
		readonly ObservableCollection<TMenuType> _menus = new ObservableCollection<TMenuType>();
		internal override IReadOnlyList<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCastingList<Element, TMenuType>(_menus);

		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(nameof(IsEnabled), typeof(bool),
			typeof(VisualElement), true);

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public Menu()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='IsEnabled']/Docs" />
		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		public TMenuType this[int index]
		{
			get { return _menus[index]; }
			set { _menus[index] = value; }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Invalidate']/Docs" />
		public void Invalidate() => OnPropertyChanged();

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Count']/Docs" />
		public int Count => _menus.Count;

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='IsReadOnly']/Docs" />
		public bool IsReadOnly => false;

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Add']/Docs" />
		public void Add(TMenuType item)
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
		public bool Contains(TMenuType item)
		{
			return _menus.Contains(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='CopyTo']/Docs" />
		public void CopyTo(TMenuType[] array, int arrayIndex)
		{
			_menus.CopyTo(array, arrayIndex);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='GetEnumerator']/Docs" />
		public IEnumerator<TMenuType> GetEnumerator()
		{
			return _menus.GetEnumerator();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='IndexOf']/Docs" />
		public int IndexOf(TMenuType item)
		{
			return _menus.IndexOf(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Insert']/Docs" />
		public void Insert(int index, TMenuType item)
		{
			_menus.Insert(index, item);
			Invalidate();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Menu.xml" path="//Member[@MemberName='Remove']/Docs" />
		public bool Remove(TMenuType item)
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
