using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	public partial class MenuBar : Element, IMenuBar
	{
		ReadOnlyCastingList<Element, IMenuBarItem> _logicalChildren;
		internal override IReadOnlyList<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCastingList<Element, IMenuBarItem>(_children);

		readonly List<IMenuBarItem> _children = new();

		public int Count => _children.Count;

		public bool IsReadOnly => false;

		public IMenuBarItem this[int index] { get => _children[index]; set => _children[index] = value; }

		public int IndexOf(IMenuBarItem item) =>
			_children.IndexOf(item);

		public void Insert(int index, IMenuBarItem item) =>
			_children.Insert(index, item);

		public void RemoveAt(int index) =>
			_children.RemoveAt(index);

		public void Add(IMenuBarItem item) =>
			_children.Add(item);

		public void Clear() =>
			_children.Clear();

		public bool Contains(IMenuBarItem item) =>
			_children.Contains(item);

		public void CopyTo(IMenuBarItem[] array, int arrayIndex) =>
			_children.CopyTo(array, arrayIndex);

		public bool Remove(IMenuBarItem item) =>
			_children.Remove(item);

		public IEnumerator<IMenuBarItem> GetEnumerator() =>
			_children.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			_children.GetEnumerator();
	}
}