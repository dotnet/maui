using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Internal
{
	class WeakList<T> : IList<T?>
	{
		readonly List<WeakReference> _items = new();

		public T? this[int index]
		{
			get => (T?)_items[index].Target;
			set => throw new NotImplementedException();
		}

		public int Count => CleanseItems().Count;

		public bool IsReadOnly => false;

		public void Add(T? item)
		{
			if (item == null)
				return;

			if (Contains(item))
				return;

			_items.Add(new WeakReference(item));
		}

		public void Clear()
		{
			CleanseItems();

			//foreach (var item in views)
			//{
			//	(item.Target as IDisposable)?.Dispose();
			//}

			_items.Clear();
		}

		public bool Contains(T? item)
		{
			if (item == null)
				return false;

			var items = new List<WeakReference>(CleanseItems());
			foreach (var x in items)
			{
				if (x.IsAlive && EqualityComparer<T>.Default.Equals(item, (T)x.Target!))
					return true;
			}

			return false;
		}

		public void CopyTo(T?[] array, int arrayIndex) =>
			throw new NotImplementedException();

		public IEnumerator<T> GetEnumerator()
		{
			var items = new List<WeakReference>(CleanseItems());
			foreach (var x in items)
				yield return (T)x.Target!;
		}

		public int IndexOf(T? item) =>
			throw new NotImplementedException();

		public void Insert(int index, T? item) =>
			throw new NotImplementedException();

		public bool Remove(T? item)
		{
			if (item == null)
				return false;

			var removed = _items.RemoveAll(x => EqualityComparer<T>.Default.Equals(item, (T)x.Target!));

			return removed > 0;
		}

		public void RemoveAt(int index) =>
			throw new NotImplementedException();

		public void ForEach(Action<T> action)
		{
			var items = new List<WeakReference>(CleanseItems());
			foreach (var item in items)
			{
				if (item.IsAlive)
					action?.Invoke((T)item.Target!);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		List<WeakReference> CleanseItems()
		{
			_items.RemoveAll(x => !x.IsAlive);
			return _items;
		}
	}
}