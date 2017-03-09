using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NotifyCollectionChangedEventArgsEx : NotifyCollectionChangedEventArgs
	{
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action) : base(action)
		{
			Count = count;
		}

		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList changedItems) : base(action, changedItems)
		{
			Count = count;
		}

		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList newItems, IList oldItems) : base(action, newItems, oldItems)
		{
			Count = count;
		}

		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex) : base(action, newItems, oldItems, startingIndex)
		{
			Count = count;
		}

		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList changedItems, int startingIndex) : base(action, changedItems, startingIndex)
		{
			Count = count;
		}

		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex) : base(action, changedItems, index, oldIndex)
		{
			Count = count;
		}

		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object changedItem) : base(action, changedItem)
		{
			Count = count;
		}

		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object changedItem, int index) : base(action, changedItem, index)
		{
			Count = count;
		}

		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex) : base(action, changedItem, index, oldIndex)
		{
			Count = count;
		}

		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object newItem, object oldItem) : base(action, newItem, oldItem)
		{
			Count = count;
		}

		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object newItem, object oldItem, int index) : base(action, newItem, oldItem, index)
		{
			Count = count;
		}

		public int Count { get; private set; }
	}
}