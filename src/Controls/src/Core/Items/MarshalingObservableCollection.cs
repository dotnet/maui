#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
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
		readonly bool _coalesceChanges;
		readonly Action _refresh;
		int _refreshPending;
		int _disposed;

		/// <param name="list">The list parameter.</param>
		public MarshalingObservableCollection(IList list)
		{
			if (!(list is INotifyCollectionChanged incc))
				throw new ArgumentException($"{nameof(list)} must implement {nameof(INotifyCollectionChanged)}");

			_internalCollection = list;
			_dispatcher = Dispatcher.GetForCurrentThread();
			_internalCollectionChanged = InternalCollectionChanged;
			_proxy = new WeakNotifyCollectionChangedProxy(incc, _internalCollectionChanged);

			foreach (var item in _internalCollection)
			{
				Add(item);
			}
		}

		/// <summary>
		/// Creates a synchronized UI-thread snapshot. Background bursts are coalesced into
		/// resets at most once per dispatch window; the public constructor keeps per-change delivery.
		/// </summary>
		internal MarshalingObservableCollection(IEnumerable source, IDispatcher dispatcher)
		{
			if (source is not INotifyCollectionChanged observable)
				throw new ArgumentException($"{nameof(source)} must implement {nameof(INotifyCollectionChanged)}", nameof(source));

			_internalCollection = source;
			_dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			_coalesceChanges = true;
			_internalCollectionChanged = InternalCollectionChanged;
			_proxy = new WeakNotifyCollectionChangedProxy();
			var weakSelf = new WeakReference<MarshalingObservableCollection>(this);
			_refresh = () =>
			{
				if (weakSelf.TryGetTarget(out var collection))
					collection.RefreshSnapshot();
			};

			AccessSource(() =>
			{
				foreach (var item in source)
					Add(item);

				// The snapshot and subscription share the registered synchronization scope.
				_proxy.Subscribe(observable, _internalCollectionChanged);
			});
		}

		~MarshalingObservableCollection()
		{
			if (_coalesceChanges)
				_proxy?.Unsubscribe();
		}

		void AccessSource(Action access)
		{
			if (BindingBase.TryGetSynchronizedCollection(_internalCollection, out var synchronization))
				synchronization.Callback(_internalCollection, synchronization.Context, access, false);
			else
				access();
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
			if (_coalesceChanges)
			{
				if (Volatile.Read(ref _disposed) != 0 || Volatile.Read(ref _refreshPending) != 0)
					return;

				// Snapshot reads run after the writer's notification has returned and its lock is released.
				QueueRefresh();
				return;
			}

			if (args.Action == NotifyCollectionChangedAction.Reset)
			{
				var items = new List<object>();
				var list = (IList)_internalCollection;
				for (int n = 0; n < list.Count; n++)
				{
					items.Add(list[n]);
				}

				args = new ResetNotifyCollectionChangedEventArgs(items);
			}

			_dispatcher.DispatchIfRequired(() => HandleCollectionChange(args));
		}

		void QueueRefresh()
		{
			if (Interlocked.Exchange(ref _refreshPending, 1) == 0 &&
				!_dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(16), _refresh))
			{
				Interlocked.Exchange(ref _refreshPending, 0);
				throw new InvalidOperationException("Unable to dispatch synchronized collection updates.");
			}
		}

		void RefreshSnapshot()
		{
			if (Volatile.Read(ref _disposed) != 0)
				return;

			// Changes during capture or Reset delivery schedule another refresh, never a nested update.
			Interlocked.Exchange(ref _refreshPending, 0);
			List<object> items = null;
			AccessSource(() =>
			{
				if (_internalCollection is IList list)
				{
					int count = list.Count;
					items = new List<object>(count);
					for (int index = 0; index < count; index++)
						items.Add(list[index]);
				}
				else
				{
					items = new List<object>(Count);
					foreach (var item in _internalCollection)
						items.Add(item);
				}
			});

			if (Volatile.Read(ref _disposed) == 0)
			{
				Clear();
				AddRange(items);
				OnCollectionChanged(new ResetNotifyCollectionChangedEventArgs(items));
			}
		}

		void HandleCollectionChange(NotifyCollectionChangedEventArgs args)
		{
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

			for (int n = 0; n < count; n++)
			{
				var toMove = this[args.OldStartingIndex];
				RemoveAt(args.OldStartingIndex);
				Insert(args.NewStartingIndex, toMove);
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
			Interlocked.Exchange(ref _disposed, 1);
			_proxy.Unsubscribe();
			GC.SuppressFinalize(this);
		}
	}
}
