using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class MarshalingObservableCollectionTests
	{
		MarshalingTestPlatformServices _services;

		[SetUp]
		public void Setup()
		{
			_services = new MarshalingTestPlatformServices();
			Device.PlatformServices = _services;
			_services.Start();
		}

		[TearDown]
		public void TearDown()
		{
			_services.Stop();
			_services = null;
		}

		[Test]
		[Description("Added items don't show up until they've been processed on the UI thread")]
		public async Task AddOffUIThread()
		{
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
			var onMainThreadCount = await Device.InvokeOnMainThreadAsync<int>(() =>
			{
				return moc.Count;
			});

			Assert.That(countFromThreadPool, Is.EqualTo(0), "Count should be zero because the update on the UI thread hasn't run yet");
			Assert.That(onMainThreadCount, Is.EqualTo(1), "Count should be 1 because the UI thread has updated");
			Assert.That(insertCount, Is.EqualTo(1), "The CollectionChanged event should have fired with an Add exactly 1 time");
		}

		[Test]
		[Description("Intial item count should match wrapped collection.")]
		public async Task InitialItemCountsMatch()
		{
			var source = new ObservableCollection<int> { 1, 2 };

			var moc = new MarshalingObservableCollection(source);

			Assert.That(source.Count, Is.EqualTo(moc.Count));
		}

		[Test]
		[Description("Clears don't show up until they've been processed on the UI thread")]
		public async Task ClearOnUIThread()
		{
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
			var onMainThreadCount = await Device.InvokeOnMainThreadAsync<int>(() => moc.Count);

			Assert.That(countFromThreadPool, Is.EqualTo(2), "Count should be pre-clear");
			Assert.That(onMainThreadCount, Is.EqualTo(0), "Count should be zero because the Clear has been processed");
		}

		[Test]
		[Description("A Reset should reflect the state at the time of the Reset")]
		public async Task ClearAndAddOffUIThread()
		{
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
			var onMainThreadCount = await Device.InvokeOnMainThreadAsync<int>(() => moc.Count);

			Assert.That(countFromThreadPool, Is.EqualTo(2), "Count should be pre-clear");
			Assert.That(onMainThreadCount, Is.EqualTo(1), "Should have processed a Clear and an Add");
		}

		[Test]
		[Description("Removed items are still there until they're removed on the UI thread")]
		public async Task RemoveOffUIThread()
		{
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
			var onMainThreadCount = await Device.InvokeOnMainThreadAsync<int>(() => moc.Count);

			Assert.That(countFromThreadPool, Is.EqualTo(2), "Count should be pre-remove");
			Assert.That(onMainThreadCount, Is.EqualTo(1), "Remove has now processed");
		}

		[Test]
		[Description("Until the UI thread processes a change, the indexer should remain consistent")]
		public async Task IndexerConsistent()
		{
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
		}

		[Test]
		[Description("Don't show replacements until the UI thread has processed them")]
		public async Task ReplaceOffUIThread()
		{
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
			var onMainThreadValue = await Device.InvokeOnMainThreadAsync(() => moc[0]);

			Assert.That(itemFromThreadPool, Is.EqualTo(1), "Should have value from before replace");
			Assert.That(onMainThreadValue, Is.EqualTo(42), "Should have value from after replace");
		}

		[Test]
		[Description("Don't show moves until the UI thread has processed them")]
		public async Task MoveOffUIThread()
		{
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
			var onMainThreadValue = await Device.InvokeOnMainThreadAsync(() => moc[0]);

			Assert.That(itemFromThreadPool, Is.EqualTo(1), "Should have value from before move");
			Assert.That(onMainThreadValue, Is.EqualTo(2), "Should have value from after move");
		}

		// This class simulates running a single UI thread with a queue and non-UI threads;
		// this allows us to test IsInvokeRequired/BeginInvoke without having to be on an actual device
		class MarshalingTestPlatformServices : IPlatformServices
		{
			int _threadId;
			bool _running;
			Queue<Action> _todo = new Queue<Action>();

			public void Stop()
			{
				_running = false;
			}

			public void Start()
			{
				_running = true;
				Task.Run(() =>
				{

					if (_threadId == 0)
					{
						_threadId = Thread.CurrentThread.ManagedThreadId;
					}

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

			public bool IsInvokeRequired => Thread.CurrentThread.ManagedThreadId != _threadId;

			public void BeginInvokeOnMainThread(Action action)
			{
				_todo.Enqueue(action);
			}

			public OSAppTheme RequestedTheme { get; }
			public string RuntimePlatform { get; }

			public Ticker CreateTicker()
			{
				throw new NotImplementedException();
			}

			public Assembly[] GetAssemblies()
			{
				throw new NotImplementedException();
			}

			public Color GetNamedColor(string name)
			{
				throw new NotImplementedException();
			}

			public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
			{
				throw new NotImplementedException();
			}

			public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
			{
				throw new NotImplementedException();
			}

			public Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
			{
				throw new NotImplementedException();
			}

			public IIsolatedStorageFile GetUserStoreForApplication()
			{
				throw new NotImplementedException();
			}

			public void OpenUriAction(Uri uri)
			{
				throw new NotImplementedException();
			}

			public void QuitApplication()
			{
				throw new NotImplementedException();
			}

			public void StartTimer(TimeSpan interval, Func<bool> callback)
			{
				throw new NotImplementedException();
			}

			public string GetHash(string input)
			{
				throw new NotImplementedException();
			}

			public string GetMD5Hash(string input)
			{
				throw new NotImplementedException();
			}
		}
	}
}