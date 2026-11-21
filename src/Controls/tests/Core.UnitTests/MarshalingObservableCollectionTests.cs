using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class MarshalingObservableCollectionTests : IDisposable
	{
		MarshalingTestDispatcherProvider _dispatcherProvider;

		public MarshalingObservableCollectionTests()
		{
			_dispatcherProvider = new MarshalingTestDispatcherProvider();
			DispatcherProvider.SetCurrent(_dispatcherProvider);
		}

		public void Dispose()
		{
			DispatcherProvider.SetCurrent(null);
			_dispatcherProvider.StopAllDispatchers();
			_dispatcherProvider = null;
		}

		[Fact("Added items don't show up until they've been processed on the UI thread")]
		public Task AddOffUIThread() => DispatcherTest.Run(async () =>
		{
			var _dispatcher = Dispatcher.GetForCurrentThread();

			int insertCount = 0;
			var countFromThreadPool = -1;

			var source = new ObservableCollection<int>();
			var moc = new MarshalingObservableCollection(source);

			moc.CollectionChanged += (sender, args) =>
			{
				if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
				{
					insertCount += 1;
				}
			};

			// Add an item from a threadpool thread
			await Task.Run(() =>
			{
				source.Add(1);
				countFromThreadPool = moc.Count;
			});

			// Check the result on the main thread
			var onMainThreadCount = await _dispatcher.DispatchAsync<int>(() =>
			{
				return moc.Count;
			});

			Assert.True(countFromThreadPool == 0, "Count should be zero because the update on the UI thread hasn't run yet");
			Assert.True(onMainThreadCount == 1, "Count should be 1 because the UI thread has updated");
			Assert.True(insertCount == 1, "The CollectionChanged event should have fired with an Add exactly 1 time");
		});

		[Fact("Initial item count should match wrapped collection.")]
		public Task InitialItemCountsMatch() => DispatcherTest.Run(async () =>
		{
			var _dispatcher = Dispatcher.GetForCurrentThread();

			var source = new ObservableCollection<int> { 1, 2 };

			var moc = new MarshalingObservableCollection(source);

			Assert.Equal(source.Count, moc.Count);
		});

		[Fact("Clears don't show up until they've been processed on the UI thread")]
		public Task ClearOnUIThread() => DispatcherTest.Run(async () =>
		{
			var _dispatcher = Dispatcher.GetForCurrentThread();

			var countFromThreadPool = -1;

			var source = new ObservableCollection<int>
			{
				1,
				2
			};

			var moc = new MarshalingObservableCollection(source);

			// Call Clear from a threadpool thread
			await Task.Run(() =>
			{
				source.Clear();
				countFromThreadPool = moc.Count;
			});

			// Check the result on the main thread
			var onMainThreadCount = await _dispatcher.DispatchAsync<int>(() => moc.Count);

			Assert.True(countFromThreadPool == 2, "Count should be pre-clear");
			Assert.True(onMainThreadCount == 0, "Count should be zero because the Clear has been processed");
		});

		[Fact("A Reset should reflect the state at the time of the Reset")]
		public Task ClearAndAddOffUIThread() => DispatcherTest.Run(async () =>
		{
			var _dispatcher = Dispatcher.GetForCurrentThread();

			var countFromThreadPool = -1;

			var source = new ObservableCollection<int>
			{
				1,
				2
			};

			var moc = new MarshalingObservableCollection(source);

			// Call Clear from a threadpool thread
			await Task.Run(() =>
			{
				source.Clear();
				source.Add(4);
				countFromThreadPool = moc.Count;
			});

			// Check the result on the main thread
			var onMainThreadCount = await _dispatcher.DispatchAsync<int>(() => moc.Count);

			Assert.True(countFromThreadPool == 2, "Count should be pre-clear");
			Assert.True(onMainThreadCount == 1, "Should have processed a Clear and an Add");
		});

		[Fact("Removed items are still there until they're removed on the UI thread")]
		public Task RemoveOffUIThread() => DispatcherTest.Run(async () =>
		{
			var _dispatcher = Dispatcher.GetForCurrentThread();

			var countFromThreadPool = -1;

			var source = new ObservableCollection<int> { 1, 2 };

			var moc = new MarshalingObservableCollection(source);

			// Call Clear from a threadpool thread
			await Task.Run(() =>
			{
				source.Remove(1);
				countFromThreadPool = moc.Count;
			});

			// Check the result on the main thread
			var onMainThreadCount = await _dispatcher.DispatchAsync<int>(() => moc.Count);

			Assert.True(countFromThreadPool == 2, "Count should be pre-remove");
			Assert.True(onMainThreadCount == 1, "Remove has now processed");
		});

		[Fact("Until the UI thread processes a change, the indexer should remain consistent")]
		public Task IndexerConsistent() => DispatcherTest.Run(async () =>
		{
			var _dispatcher = Dispatcher.GetForCurrentThread();

			int itemFromThreadPool = -1;

			var source = new ObservableCollection<int> { 1, 2 };

			var moc = new MarshalingObservableCollection(source);

			// Call Remove from a threadpool thread
			await Task.Run(() =>
			{
				source.Remove(1);
				itemFromThreadPool = (int)moc[1];
			});

			Assert.True(itemFromThreadPool == 2, "Should have indexer value from before remove");
		});

		[Fact("Don't show replacements until the UI thread has processed them")]
		public Task ReplaceOffUIThread() => DispatcherTest.Run(async () =>
		{
			var _dispatcher = Dispatcher.GetForCurrentThread();

			int itemFromThreadPool = -1;

			var source = new ObservableCollection<int> { 1, 2 };

			var moc = new MarshalingObservableCollection(source);

			// Replace a value from a threadpool thread
			await Task.Run(() =>
			{
				source[0] = 42;
				itemFromThreadPool = (int)moc[0];
			});

			// Check the result on the main thread
			var onMainThreadValue = await _dispatcher.DispatchAsync(() => moc[0]);

			Assert.True(itemFromThreadPool == 1, "Should have value from before replace");
			Assert.True((int)onMainThreadValue == 42, "Should have value from after replace");
		});

		[Fact("Don't show moves until the UI thread has processed them")]
		public Task MoveOffUIThread() => DispatcherTest.Run(async () =>
		{
			var _dispatcher = Dispatcher.GetForCurrentThread();

			int itemFromThreadPool = -1;

			var source = new ObservableCollection<int> { 1, 2 };

			var moc = new MarshalingObservableCollection(source);

			// Replace a value from a threadpool thread
			await Task.Run(() =>
			{
				source.Move(1, 0);
				itemFromThreadPool = (int)moc[0];
			});

			// Check the result on the main thread
			var onMainThreadValue = await _dispatcher.DispatchAsync(() => moc[0]);

			Assert.True(itemFromThreadPool == 1, "Should have value from before move");
			Assert.True((int)onMainThreadValue == 2, "Should have value from after move");
		});



		[Theory]
		[InlineData(0, 3, 2)]
		[InlineData(3, 0, 2)]
		[InlineData(1, 1, 2)]
		public Task RangeMovePreservesOrder(int oldIndex, int newIndex, int count) => DispatcherTest.Run(async () =>
		{
			var dispatcher = Dispatcher.GetForCurrentThread();
			var source = new RangeCollection { 0, 1, 2, 3, 4, 5 };
			var collection = new MarshalingObservableCollection(source);
			int notifications = 0;
			collection.CollectionChanged += (_, args) =>
			{
				Assert.Equal(NotifyCollectionChangedAction.Move, args.Action);
				Assert.Equal(oldIndex, args.OldStartingIndex);
				Assert.Equal(newIndex, args.NewStartingIndex);
				Assert.Equal(count, args.OldItems.Count);
				notifications++;
			};

			await Task.Run(() => source.MoveRange(oldIndex, newIndex, count));
			await dispatcher.DispatchAsync(() =>
			{
				Assert.Equal(source.Count, collection.Count);
				for (int i = 0; i < source.Count; i++)
					Assert.Equal(source[i], collection[i]);
				Assert.Equal(1, notifications);
			});
		});

		[Theory]
		[InlineData(NotifyCollectionChangedAction.Add)]
		[InlineData(NotifyCollectionChangedAction.Remove)]
		[InlineData(NotifyCollectionChangedAction.Replace)]
		[InlineData(NotifyCollectionChangedAction.Move)]
		public Task IndexlessChangesCaptureResetBeforeFollowingAdd(NotifyCollectionChangedAction action) => DispatcherTest.Run(async () =>
		{
			var dispatcher = Dispatcher.GetForCurrentThread();
			var source = new RangeCollection { 0, 1, 2 };
			var collection = new MarshalingObservableCollection(source);
			var notifications = new List<NotifyCollectionChangedAction>();
			collection.CollectionChanged += (_, args) => notifications.Add(args.Action);

			await Task.Run(() =>
			{
				source.ChangeWithoutIndex(action);
				source.Add(99);
			});

			await dispatcher.DispatchAsync(() =>
			{
				Assert.Equal(source.Count, collection.Count);
				for (int i = 0; i < source.Count; i++)
					Assert.Equal(source[i], collection[i]);
				Assert.Equal(new[] { NotifyCollectionChangedAction.Reset, NotifyCollectionChangedAction.Add }, notifications);
			});
		});

		class RangeCollection : ObservableCollection<int>
		{
			public void ChangeWithoutIndex(NotifyCollectionChangedAction action)
			{
				object oldItem = Items[0];
				switch (action)
				{
					case NotifyCollectionChangedAction.Add:
						Items.Add(7);
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, 7));
						break;
					case NotifyCollectionChangedAction.Remove:
						Items.RemoveAt(0);
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, oldItem));
						break;
					case NotifyCollectionChangedAction.Replace:
						Items[0] = 7;
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, 7, oldItem));
						break;
					case NotifyCollectionChangedAction.Move:
						Items.RemoveAt(0);
						Items.Insert(2, (int)oldItem);
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, oldItem, 2, -1));
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(action));
				}
			}

			public void MoveRange(int oldIndex, int newIndex, int count)
			{
				var moved = new List<int>();
				for (int i = 0; i < count; i++)
				{
					moved.Add(Items[oldIndex]);
					Items.RemoveAt(oldIndex);
				}

				for (int i = 0; i < moved.Count; i++)
					Items.Insert(newIndex + i, moved[i]);

				OnCollectionChanged(new NotifyCollectionChangedEventArgs(
					NotifyCollectionChangedAction.Move, moved, newIndex, oldIndex));
			}
		}

		// This class simulates running a single UI thread with a queue and non-UI threads;
		// this allows us to test IsInvokeRequired/BeginInvoke without having to be on an actual device
		class MarshalingTestDispatcherProvider : IDispatcherProvider
		{
			ThreadLocal<MarshalingTestDispatcher> s_dispatcherInstance = new(() =>
			{
				var dispatcher = new MarshalingTestDispatcher();
				dispatcher.Start();
				return dispatcher;
			}, true);

			public IDispatcher GetForCurrentThread() =>
				s_dispatcherInstance.Value;

			public void StopAllDispatchers()
			{
				foreach (var dispatcher in s_dispatcherInstance.Values)
					dispatcher.Stop();
			}

			class MarshalingTestDispatcher : IDispatcher
			{
				int _threadId;
				bool _running;
				Queue<Action> _todo = new();

				public void Stop()
				{
					_running = false;
				}

				public void Start()
				{
					_running = true;

					Task.Run(() =>
					{
						_threadId = Thread.CurrentThread.ManagedThreadId;

						while (_running)
						{
							try
							{
								Thread.Sleep(100);

								while (_todo.Count > 0)
								{
									_todo.Dequeue().Invoke();
								}
							}
							catch (Exception ex)
							{
								Stop();
							}
						}
					});
				}

				public bool IsDispatchRequired =>
					Thread.CurrentThread.ManagedThreadId != _threadId;

				public bool Dispatch(Action action)
				{
					if (!_running)
						throw new InvalidOperationException("Dispatcher has been stopped.");

					_todo.Enqueue(action);
					return true;
				}

				public bool DispatchDelayed(TimeSpan delay, Action action) =>
					throw new NotImplementedException();

				public IDispatcherTimer CreateTimer() =>
					throw new NotImplementedException();
			}
		}
	}
}