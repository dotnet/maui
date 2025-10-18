#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NSubstitute;
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

    public partial class MarshalingObservableCollectionConstructorTests : IDisposable
    {
        MarshalingTestDispatcherProvider _dispatcherProvider;

        public MarshalingObservableCollectionConstructorTests()
        {
            _dispatcherProvider = new MarshalingTestDispatcherProvider();
            DispatcherProvider.SetCurrent(_dispatcherProvider);
        }

        public void Dispose()
        {
            DispatcherProvider.SetCurrent(null);
            _dispatcherProvider?.StopAllDispatchers();
            _dispatcherProvider = null;
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentException when passed an IList that does not implement INotifyCollectionChanged.
        /// This test covers the validation logic at line 24-25 of the constructor.
        /// Expected result: ArgumentException with appropriate message.
        /// </summary>
        [Fact]
        public void Constructor_ListDoesNotImplementINotifyCollectionChanged_ThrowsArgumentException()
        {
            // Arrange
            var listWithoutNotifyChanged = Substitute.For<IList>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new MarshalingObservableCollection(listWithoutNotifyChanged));
            Assert.Contains("list", exception.Message);
            Assert.Contains("INotifyCollectionChanged", exception.Message);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when passed a null list parameter.
        /// This test validates null parameter handling.
        /// Expected result: ArgumentNullException or NullReferenceException during validation.
        /// </summary>
        [Fact]
        public void Constructor_NullList_ThrowsException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsAny<Exception>(() => new MarshalingObservableCollection(null));
        }

        /// <summary>
        /// Tests that the constructor successfully initializes with an empty ObservableCollection.
        /// This test validates successful initialization with a valid empty collection.
        /// Expected result: Empty MarshalingObservableCollection is created without exceptions.
        /// </summary>
        [Fact]
        public void Constructor_EmptyObservableCollection_InitializesSuccessfully()
        {
            // Arrange
            var emptyCollection = new ObservableCollection<object>();

            // Act
            var marshalingCollection = new MarshalingObservableCollection(emptyCollection);

            // Assert
            Assert.NotNull(marshalingCollection);
            Assert.Empty(marshalingCollection);
        }

        /// <summary>
        /// Tests that the constructor successfully initializes with a populated ObservableCollection.
        /// This test validates that initial items from the source collection are copied to the marshaling collection.
        /// Expected result: MarshalingObservableCollection contains the same items as the source collection.
        /// </summary>
        [Fact]
        public void Constructor_PopulatedObservableCollection_CopiesInitialItems()
        {
            // Arrange
            var sourceItems = new[] { "item1", 42, new object() };
            var populatedCollection = new ObservableCollection<object>(sourceItems);

            // Act
            var marshalingCollection = new MarshalingObservableCollection(populatedCollection);

            // Assert
            Assert.NotNull(marshalingCollection);
            Assert.Equal(sourceItems.Length, marshalingCollection.Count);
            Assert.Equal(sourceItems[0], marshalingCollection[0]);
            Assert.Equal(sourceItems[1], marshalingCollection[1]);
            Assert.Equal(sourceItems[2], marshalingCollection[2]);
        }

        /// <summary>
        /// Tests that the constructor works with different types of collections that implement both IList and INotifyCollectionChanged.
        /// This test validates compatibility with various collection types.
        /// Expected result: MarshalingObservableCollection is successfully created for all valid collection types.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void Constructor_ValidCollectionTypes_InitializesWithCorrectItemCount(int itemCount)
        {
            // Arrange
            var collection = new ObservableCollection<object>();
            for (int i = 0; i < itemCount; i++)
            {
                collection.Add($"item{i}");
            }

            // Act
            var marshalingCollection = new MarshalingObservableCollection(collection);

            // Assert
            Assert.NotNull(marshalingCollection);
            Assert.Equal(itemCount, marshalingCollection.Count);
        }

        /// <summary>
        /// Tests that the constructor properly subscribes to CollectionChanged events of the source collection.
        /// This test validates that the event subscription is established during construction.
        /// Expected result: The source collection's CollectionChanged event is subscribed to.
        /// </summary>
        [Fact]
        public void Constructor_ValidCollection_SubscribesToCollectionChangedEvent()
        {
            // Arrange
            var mockCollection = Substitute.For<IList, INotifyCollectionChanged>();
            mockCollection.GetEnumerator().Returns(new List<object>().GetEnumerator());

            // Act
            var marshalingCollection = new MarshalingObservableCollection(mockCollection);

            // Assert
            Assert.NotNull(marshalingCollection);
            ((INotifyCollectionChanged)mockCollection).Received(1).CollectionChanged += Arg.Any<NotifyCollectionChangedEventHandler>();
        }

        /// <summary>
        /// Tests constructor behavior with collections containing null items.
        /// This test validates that null items in the source collection are properly handled.
        /// Expected result: Null items are copied to the marshaling collection without issues.
        /// </summary>
        [Fact]
        public void Constructor_CollectionWithNullItems_HandlesNullItemsCorrectly()
        {
            // Arrange
            var collectionWithNulls = new ObservableCollection<object> { "item1", null, "item3", null };

            // Act
            var marshalingCollection = new MarshalingObservableCollection(collectionWithNulls);

            // Assert
            Assert.NotNull(marshalingCollection);
            Assert.Equal(4, marshalingCollection.Count);
            Assert.Equal("item1", marshalingCollection[0]);
            Assert.Null(marshalingCollection[1]);
            Assert.Equal("item3", marshalingCollection[2]);
            Assert.Null(marshalingCollection[3]);
        }

        // Helper test dispatcher provider similar to the existing tests
        class MarshalingTestDispatcherProvider : IDispatcherProvider
        {
            List<MarshalingTestDispatcher> _dispatchers = new List<MarshalingTestDispatcher>();

            public IDispatcher GetForCurrentThread()
            {
                var dispatcher = new MarshalingTestDispatcher();
                _dispatchers.Add(dispatcher);
                return dispatcher;
            }

            public void StopAllDispatchers()
            {
                foreach (var dispatcher in _dispatchers)
                {
                    dispatcher.Stop();
                }
                _dispatchers.Clear();
            }

            class MarshalingTestDispatcher : IDispatcher
            {
                bool _running = true;

                public void Stop()
                {
                    _running = false;
                }

                public bool IsDispatchRequired => false;

                public bool Dispatch(Action action)
                {
                    if (_running)
                    {
                        action?.Invoke();
                        return true;
                    }
                    return false;
                }

                public bool DispatchDelayed(TimeSpan delay, Action action)
                {
                    return Dispatch(action);
                }

                public IDispatcherTimer CreateTimer()
                {
                    return Substitute.For<IDispatcherTimer>();
                }
            }
        }
    }
}