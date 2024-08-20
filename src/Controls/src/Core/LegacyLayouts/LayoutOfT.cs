#nullable disable
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility
{
	public abstract partial class Layout<T>
	{
		int ICollection<IView>.Count => _children.Count;
		bool ICollection<IView>.IsReadOnly => ((ICollection<IView>)_children).IsReadOnly;
		public IView this[int index] { get => _children[index]; set => _children[index] = (T)value; }

		bool Maui.ILayout.ClipsToBounds => IsClippedToBounds;

		void ICollection<IView>.Add(IView child)
		{
			if (child is T view)
			{
				_children.Add(view);
			}
		}

		bool ICollection<IView>.Remove(IView child)
		{
			if (child is T view)
			{
				_children.Remove(view);
				return true;
			}

			return false;
		}

		int IList<IView>.IndexOf(IView child)
		{
			return _children.IndexOf(child);
		}

		void IList<IView>.Insert(int index, IView child)
		{
			if (child is T view)
			{
				_children.Insert(index, view);
			}
		}

		void IList<IView>.RemoveAt(int index)
		{
			_children.RemoveAt(index);
		}

		void ICollection<IView>.Clear()
		{
			_children.Clear();
		}

		bool ICollection<IView>.Contains(IView child)
		{
			return _children.Contains(child);
		}

		void ICollection<IView>.CopyTo(IView[] array, int arrayIndex)
		{
			_children.CopyTo(array, arrayIndex);
		}

		IEnumerator<IView> IEnumerable<IView>.GetEnumerator()
		{
			return _children.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _children.GetEnumerator();
		}
	}
}
