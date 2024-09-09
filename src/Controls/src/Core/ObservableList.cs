#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	internal class ObservableList<T> : ObservableCollection<T>
	{
		// There's lots of special-casing optimizations that could be done here
		// but right now this is only being used for tests.

		public void AddRange(IEnumerable<T> range)
		{
			if (range == null)
				throw new ArgumentNullException(nameof(range));

			List<T> items = range.ToList();
			int index = Items.Count;
			foreach (T item in items)
				Items.Add(item);

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index));
		}

		public void InsertRange(int index, IEnumerable<T> range)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException(nameof(index));
			if (range == null)
				throw new ArgumentNullException(nameof(range));

			int originalIndex = index;

			List<T> items = range.ToList();
			foreach (T item in items)
				Items.Insert(index++, item);

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, originalIndex));
		}

		public void Move(int oldIndex, int newIndex, int count)
		{
			if (oldIndex < 0 || oldIndex + count > Count)
				throw new ArgumentOutOfRangeException(nameof(oldIndex));
			if (newIndex < 0 || newIndex + count > Count)
				throw new ArgumentOutOfRangeException(nameof(newIndex));

			var items = new List<T>(count);
			for (var i = 0; i < count; i++)
			{
				T item = Items[oldIndex];
				items.Add(item);
				Items.RemoveAt(oldIndex);
			}

			int index = newIndex;
			if (newIndex > oldIndex)
				index -= items.Count - 1;

			for (var i = 0; i < items.Count; i++)
				Items.Insert(index + i, items[i]);

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, newIndex, oldIndex));
		}

		public void RemoveAt(int index, int count)
		{
			if (index < 0 || index + count > Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			T[] items = Items.Skip(index).Take(count).ToArray();
			for (int i = index; i < count; i++)
				Items.RemoveAt(i);

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, index));
		}

		public void RemoveRange(IEnumerable<T> range)
		{
			if (range == null)
				throw new ArgumentNullException(nameof(range));

			List<T> items = range.ToList();
			foreach (T item in items)
				Items.Remove(item);

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items));
		}

		public void ReplaceRange(int startIndex, IEnumerable<T> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			T[] ritems = items.ToArray();

			if (startIndex < 0 || startIndex + ritems.Length > Count)
				throw new ArgumentOutOfRangeException(nameof(startIndex));

			var oldItems = new T[ritems.Length];
			for (var i = 0; i < ritems.Length; i++)
			{
				oldItems[i] = Items[i + startIndex];
				Items[i + startIndex] = ritems[i];
			}

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ritems, oldItems, startIndex));
		}
	}
}