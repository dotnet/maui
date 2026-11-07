#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	// Wraps a List which implements INotifyCollectionChanged (usually an ObservableCollection)
	// and marshals all of the list modifications to the main thread. Modifications to the underlying 
	// collection which are made off of the main thread remain invisible to consumers on the main thread
	// until they have been processed by the main thread.

	/// <summary>
	/// A thread-safe observable collection that marshals all collection changes to the main thread.
	/// </summary>
	/// <remarks>
	/// This collection wraps an <see cref="INotifyCollectionChanged"/> collection and ensures that all collection change notifications
	/// are processed on the main UI thread, making it safe to bind to UI controls even when the underlying collection is modified from background threads.
	/// </remarks>
	public class MarshalingObservableCollection : List<object>, INotifyCollectionChanged
	{
		readonly IEnumerable _internalCollection;
		readonly IDispatcher _dispatcher;
		readonly WeakNotifyCollectionChangedProxy _proxy;
		readonly NotifyCollectionChangedEventHandler _internalCollectionChanged;
		bool _disposed;

		/// <param name="list">The list parameter.</param>
		public MarshalingObservableCollection(IList list)
			: this(list, Dispatcher.GetForCurrentThread())
		{
		}

		internal MarshalingObservableCollection(IEnumerable list, IDispatcher dispatcher)
		{
			if (!(list is INotifyCollectionChanged incc))
				throw new ArgumentException($"{nameof(list)} must implement {nameof(INotifyCollectionChanged)}");

			_internalCollection = list;
			_dispatcher = dispatcher;
			_internalCollectionChanged = InternalCollectionChanged;
			_proxy = new WeakNotifyCollectionChangedProxy(incc, _internalCollectionChanged);

			foreach (var item in _internalCollection)
			{
				Add(item);
			}
		}

		class ResetNotifyCollectionChangedEventArgs : NotifyCollectionChangedEventArgs
		{
			public IList Items { get; }
			public ResetNotifyCollectionChangedEventArgs(IList items)
				: base(NotifyCollectionChangedAction.Reset) => Items = items;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			CollectionChanged?.Invoke(this, args);
		}

		void InternalCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Reset ||
				(args.NewStartingIndex < 0 && args.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace or NotifyCollectionChangedAction.Move) ||
				(args.OldStartingIndex < 0 && args.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace or NotifyCollectionChangedAction.Move))
			{
				var items = new List<object>();
				if (_internalCollection is IList list)
				{
					for (int n = 0; n < list.Count; n++)
						items.Add(list[n]);
				}
				else
				{
					foreach (var item in _internalCollection)
						items.Add(item);
				}

				args = new ResetNotifyCollectionChangedEventArgs(items);
			}

			_dispatcher.DispatchIfRequired(() => HandleCollectionChange(args));
		}

		void HandleCollectionChange(NotifyCollectionChangedEventArgs args)
		{
			if (_disposed)
				return;

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Move:
					Move(args);
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
					Replace(args);
					break;
				case NotifyCollectionChangedAction.Reset:
					Reset(args);
					break;
			}
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var count = args.OldItems.Count;

			if (count == 1)
			{
				var toMove = this[args.OldStartingIndex];
				RemoveAt(args.OldStartingIndex);
				Insert(args.NewStartingIndex, toMove);
			}
			else
			{
				var toMove = GetRange(args.OldStartingIndex, count);
				RemoveRange(args.OldStartingIndex, count);
				InsertRange(args.NewStartingIndex, toMove);
			}

			OnCollectionChanged(args);
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex + args.OldItems.Count - 1;
			for (int n = startIndex; n >= args.OldStartingIndex; n--)
			{
				RemoveAt(n);
			}

			OnCollectionChanged(args);
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex;
			foreach (var item in args.NewItems)
			{
				this[startIndex] = item;
				startIndex += 1;
			}

			OnCollectionChanged(args);
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex;
			foreach (var item in args.NewItems)
			{
				Insert(startIndex, item);
				startIndex += 1;
			}

			OnCollectionChanged(args);
		}

		void Reset(NotifyCollectionChangedEventArgs args)
		{
			if (!(args is ResetNotifyCollectionChangedEventArgs resetArgs))
			{
				throw new InvalidOperationException($"Cannot guarantee collection accuracy for Resets which do not use {nameof(ResetNotifyCollectionChangedEventArgs)}");
			}

			Clear();
			foreach (var item in resetArgs.Items)
			{
				Add(item);
			}

			OnCollectionChanged(args);
		}

		internal void Dispose()
		{
			_disposed = true;
			_proxy.Unsubscribe();
		}
	}
}
