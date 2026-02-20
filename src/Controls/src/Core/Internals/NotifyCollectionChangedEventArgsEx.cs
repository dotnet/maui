#nullable disable
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>For internal use by platform renderers.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NotifyCollectionChangedEventArgsEx : NotifyCollectionChangedEventArgs
	{
		/// <summary>Creates event args with the specified count and action.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action) : base(action)
		{
			Count = count;
		}

		/// <summary>Creates event args with the specified count, action, and changed items.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList changedItems) : base(action, changedItems)
		{
			Count = count;
		}

		/// <summary>Creates event args for replace operations.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList newItems, IList oldItems) : base(action, newItems, oldItems)
		{
			Count = count;
		}

		/// <summary>Creates event args for replace operations at a specific index.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex) : base(action, newItems, oldItems, startingIndex)
		{
			Count = count;
		}

		/// <summary>Creates event args with changed items at a specific index.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList changedItems, int startingIndex) : base(action, changedItems, startingIndex)
		{
			Count = count;
		}

		/// <summary>Creates event args for move operations.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex) : base(action, changedItems, index, oldIndex)
		{
			Count = count;
		}

		/// <summary>Creates event args for a single changed item.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object changedItem) : base(action, changedItem)
		{
			Count = count;
		}

		/// <summary>Creates event args for a single changed item at a specific index.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object changedItem, int index) : base(action, changedItem, index)
		{
			Count = count;
		}

		/// <summary>Creates event args for moving a single item.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex) : base(action, changedItem, index, oldIndex)
		{
			Count = count;
		}

		/// <summary>Creates event args for replacing a single item.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object newItem, object oldItem) : base(action, newItem, oldItem)
		{
			Count = count;
		}

		/// <summary>Creates event args for replacing a single item at a specific index.</summary>
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object newItem, object oldItem, int index) : base(action, newItem, oldItem, index)
		{
			Count = count;
		}

		/// <summary>For internal use by platform renderers.</summary>
		public int Count { get; private set; }
	}
}