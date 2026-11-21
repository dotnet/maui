using System;
using System.Collections.Generic;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal sealed class ItemsViewCollectionChangedDispatcher : IDispatcher, IDisposable
	{
		readonly IDispatcher _dispatcher;
		readonly Action _drain;
		readonly Queue<Action> _pending = new();
		RecyclerView? _recyclerView;
		volatile bool _scheduled;
		bool _disposed;

		public ItemsViewCollectionChangedDispatcher(IDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
			_drain = Drain;
		}

		public bool IsDispatchRequired => _dispatcher.IsDispatchRequired || _scheduled || _recyclerView?.IsComputingLayout == true;

		// EmptyView temporarily replaces the adapter, but its collection still belongs to this RecyclerView.
		internal void Attach(RecyclerView recyclerView) => _recyclerView = recyclerView;

		public bool Dispatch(Action action)
		{
			lock (_pending)
			{
				if (_disposed)
					return false;

				_pending.Enqueue(action);
				if (_scheduled)
					return true;

				_scheduled = true;
			}

			if (_dispatcher.Dispatch(_drain))
				return true;

			Dispose();
			throw new InvalidOperationException("Unable to dispatch CollectionView collection changes.");
		}

		void Drain()
		{
			while (true)
			{
				Action action;
				lock (_pending)
				{
					if (_pending.Count == 0)
					{
						_scheduled = false;
						return;
					}

					if (_recyclerView?.IsComputingLayout == true)
					{
						if (!_dispatcher.Dispatch(_drain))
						{
							Dispose();
							throw new InvalidOperationException("Unable to dispatch CollectionView collection changes.");
						}
						return;
					}

					action = _pending.Dequeue();
				}

				try
				{
					action();
				}
				catch
				{
					Dispose();
					throw;
				}
			}
		}

		public bool DispatchDelayed(TimeSpan delay, Action action) => _dispatcher.DispatchDelayed(delay, action);

		public IDispatcherTimer CreateTimer() => _dispatcher.CreateTimer();

		public void Dispose()
		{
			lock (_pending)
			{
				_disposed = true;
				_pending.Clear();
				_recyclerView = null;
				_scheduled = false;
			}
		}
	}
}
