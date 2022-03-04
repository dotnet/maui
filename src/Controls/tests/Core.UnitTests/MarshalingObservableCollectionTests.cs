using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class MarshalingObservableCollectionTests
	{
		MarshalingTestDispatcherProvider _dispatcherProvider;

		[SetUp]
		public void Setup()
		{
			_dispatcherProvider = new MarshalingTestDispatcherProvider();
			DispatcherProvider.SetCurrent(_dispatcherProvider);
		}

		[TearDown]
		public void TearDown()
		{
			DispatcherProvider.SetCurrent(null);
			_dispatcherProvider.StopAllDispatchers();
			_dispatcherProvider = null;
		}

		[Test]
		[Description("Added items don't show up until they've been processed on the UI thread")]
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

			Assert.That(countFromThreadPool, Is.EqualTo(0), "Count should be zero because the update on the UI thread hasn't run yet");
			Assert.That(onMainThreadCount, Is.EqualTo(1), "Count should be 1 because the UI thread has updated");
			Assert.That(insertCount, Is.EqualTo(1), "The CollectionChanged event should have fired with an Add exactly 1 time");
		});

		[Test]
		[Description("Intial item count should match wrapped collection.")]
		public Task InitialItemCountsMatch() => DispatcherTest.Run(async () =>
		{
			var _dispatcher = Dispatcher.GetForCurrentThread();

			var source = new ObservableCollection<int> { 1, 2 };

			var moc = new MarshalingObservableCollection(source);

			Assert.That(source.Count, Is.EqualTo(moc.Count));
		});

		[Test]
		[Description("Clears don't show up until they've been processed on the UI thread")]
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

			Assert.That(countFromThreadPool, Is.EqualTo(2), "Count should be pre-clear");
			Assert.That(onMainThreadCount, Is.EqualTo(0), "Count should be zero because the Clear has been processed");
		});

		[Test]
		[Description("A Reset should reflect the state at the time of the Reset")]
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

			Assert.That(countFromThreadPool, Is.EqualTo(2), "Count should be pre-clear");
			Assert.That(onMainThreadCount, Is.EqualTo(1), "Should have processed a Clear and an Add");
		});

		[Test]
		[Description("Removed items are still there until they're removed on the UI thread")]
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

			Assert.That(countFromThreadPool, Is.EqualTo(2), "Count should be pre-remove");
			Assert.That(onMainThreadCount, Is.EqualTo(1), "Remove has now processed");
		});

		[Test]
		[Description("Until the UI thread processes a change, the indexer should remain consistent")]
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

			Assert.That(itemFromThreadPool, Is.EqualTo(2), "Should have indexer value from before remove");
		});

		[Test]
		[Description("Don't show replacements until the UI thread has processed them")]
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

			Assert.That(itemFromThreadPool, Is.EqualTo(1), "Should have value from before replace");
			Assert.That(onMainThreadValue, Is.EqualTo(42), "Should have value from after replace");
		});

		[Test]
		[Description("Don't show moves until the UI thread has processed them")]
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

			Assert.That(itemFromThreadPool, Is.EqualTo(1), "Should have value from before move");
			Assert.That(onMainThreadValue, Is.EqualTo(2), "Should have value from after move");
		});

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