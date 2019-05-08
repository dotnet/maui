using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class MultiTestObservableCollection<T> : List<T>, INotifyCollectionChanged
	{
		// This is a testing class which implements INotifyCollectionChanged and, unlike the regular
		// ObservableCollection, will actually fire Add and Remove with multiple items at once

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void TestAddWithList(IEnumerable<T> newItems, int insertAt)
		{
			var list = newItems.ToList();
			InsertRange(insertAt, list);
			OnNotifyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list));
		}

		public void TestRemoveWithList(int removeStart, int count)
		{
			var list = new List<T>(GetRange(removeStart, count));
			RemoveRange(removeStart, count);
			OnNotifyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list));
		}

		public void TestAddWithListAndIndex(IEnumerable<T> newItems, int insertAt)
		{
			var list = newItems.ToList();
			InsertRange(insertAt, newItems);
			OnNotifyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, insertAt));
		}

		public void TestRemoveWithListAndIndex(int removeStart, int count)
		{
			var list = new List<T>(GetRange(removeStart, count));
			RemoveRange(removeStart, count);
			OnNotifyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list, removeStart));
		}

		public void TestMoveWithList(int moveFrom, int count, int moveTo)
		{
			var movedItems = new List<T>(GetRange(moveFrom, count));

			RemoveRange(moveFrom, count);
			InsertRange(moveTo, movedItems);

			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, movedItems, moveTo, moveFrom);
			OnNotifyCollectionChanged(this, args);
		}

		public void TestReplaceWithList(int index, int count, IEnumerable<T> newItems)
		{
			var oldList = new List<T>(GetRange(index, count));
			var newList = newItems.ToList();

			RemoveRange(index, count);
			InsertRange(index, newItems);

			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newList, oldList);
			OnNotifyCollectionChanged(this, args);
		}

		public void TestReplaceWithListAndIndex(int index, int count, IEnumerable<T> newItems)
		{
			var oldList = new List<T>(GetRange(index, count));
			var newList = newItems.ToList();

			RemoveRange(index, count);
			InsertRange(index, newItems);

			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newList, oldList, index);
			OnNotifyCollectionChanged(this, args);
		}

		public void TestReset()
		{
			RemoveRange(0, Count);

			var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			OnNotifyCollectionChanged(this, args);
		}

		void OnNotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs) => CollectionChanged?.Invoke(this, eventArgs);
	}
}