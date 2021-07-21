using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class LayoutStub : StubBase, ILayout
	{
		IList<IView> _children = new List<IView>();

		public ILayoutHandler LayoutHandler => Handler as ILayoutHandler;

		public void Add(IView child)
		{
			_children.Add(child);
		}

		public bool Remove(IView child)
		{
			return _children.Remove(child);
		}

		public int IndexOf(IView item)
		{
			return _children.IndexOf(item);
		}

		public void Insert(int index, IView item)
		{
			_children.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_children.RemoveAt(index);
		}

		public void Clear()
		{
			_children.Clear();
		}

		public bool Contains(IView item)
		{
			return _children.Contains(item);
		}

		public void CopyTo(IView[] array, int arrayIndex)
		{
			_children.CopyTo(array, arrayIndex);
		}

		public IEnumerator<IView> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public Thickness Padding { get; set; }
		public int Count { get; }
		public bool IsReadOnly { get; }

		public IView this[int index] { get => _children[index]; set => _children[index] = value; }
	}
}
