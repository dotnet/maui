using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	public partial class MenuBarItem : BaseMenuItem, IMenuBarItem
	{
		ReadOnlyCastingList<Element, IMenuFlyoutItemBase> _logicalChildren;
		internal override IReadOnlyList<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCastingList<Element, IMenuFlyoutItemBase>(_children);

		readonly List<IMenuFlyoutItemBase> _children = new();


		public static readonly BindableProperty TextProperty =
			BindableProperty.Create(nameof(Text), typeof(string), typeof(MenuBarItem), null);

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}


		public int Count => _children.Count;

		public bool IsReadOnly => false;

		public IMenuFlyoutItemBase this[int index] { get => _children[index]; set => _children[index] = value; }


		public int IndexOf(IMenuFlyoutItemBase item) =>
			_children.IndexOf(item);

		public void Insert(int index, IMenuFlyoutItemBase item) =>
			_children.Insert(index, item);

		public void RemoveAt(int index) =>
			_children.RemoveAt(index);

		public void Add(IMenuFlyoutItemBase item) =>
			_children.Add(item);

		public void Clear() =>
			_children.Clear();

		public bool Contains(IMenuFlyoutItemBase item) =>
			_children.Contains(item);

		public void CopyTo(IMenuFlyoutItemBase[] array, int arrayIndex) =>
			_children.CopyTo(array, arrayIndex);

		public bool Remove(IMenuFlyoutItemBase item) =>
			_children.Remove(item);

		public IEnumerator<IMenuFlyoutItemBase> GetEnumerator() =>
			_children.GetEnumerator();

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
			_children.GetEnumerator();
	}
}