#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ListProxyTests : BaseTestFixture
    {
        [Fact]
        public void ListCount()
        {
            var list = new List<string> { "foo", "bar" };
            var proxy = new ListProxy(list);

            Assert.Equal(list.Count, proxy.Count);
            list.Add("baz");
            Assert.Equal(list.Count, proxy.Count);
        }

        [Fact]
        public void CollectionCount()
        {
            var list = new Collection<string> { "foo", "bar" };
            var proxy = new ListProxy(list);

            Assert.Equal(list.Count, proxy.Count);
            list.Add("baz");
            Assert.Equal(list.Count, proxy.Count);
        }

        [Fact("Count should ensure that the window is created if neccessary")]
        public void EnumerableInitialCount()
        {
            var enumerable = Enumerable.Range(0, 100);
            var proxy = new ListProxy(enumerable, 10);

            Assert.Equal(10, proxy.Count);
        }

        [Fact]
        public void EnumerableCount()
        {
            var enumerable = Enumerable.Range(0, 100);
            var proxy = new ListProxy(enumerable, 10);

            int changed = 0;
            proxy.CountChanged += (o, e) => changed++;

            var enumerator = proxy.GetEnumerator();
            enumerator.MoveNext();

            Assert.Equal(10, proxy.Count);
            Assert.Equal(1, changed);

            enumerator.MoveNext();

            Assert.Equal(10, proxy.Count);
            Assert.Equal(1, changed);

            while (enumerator.MoveNext())
            {
            }

            enumerator.Dispose();

            Assert.Equal(100, proxy.Count);
            Assert.Equal(19, changed);

            using (enumerator = proxy.GetEnumerator())
            {

                Assert.Equal(100, proxy.Count);

                while (enumerator.MoveNext())
                    Assert.Equal(100, proxy.Count);

                Assert.Equal(100, proxy.Count);
            }

            Assert.Equal(19, changed);
        }

        [Fact]
        public void InsideWindowSize()
        {
            var numbers = Enumerable.Range(0, 100);
            var proxy = new ListProxy(numbers, 10);

            int i = (int)proxy[5];
            Assert.Equal(5, i);
        }

        [Fact]
        public void IndexOutsideWindowSize()
        {
            var numbers = Enumerable.Range(0, 100);
            var proxy = new ListProxy(numbers, 10);

            int i = (int)proxy[50];
            Assert.Equal(50, i);
        }

        [Fact]
        public void IndexInsideToOutsideWindowSize()
        {
            var numbers = Enumerable.Range(0, 100);
            var proxy = new ListProxy(numbers, 10);

            int i = (int)proxy[5];
            Assert.Equal(5, i);

            i = (int)proxy[50];
            Assert.Equal(50, i);
        }

        [Fact]
        public void IndexOutsideToPreWindowSize()
        {
            var numbers = Enumerable.Range(0, 100);
            var proxy = new ListProxy(numbers, 10);

            int i = (int)proxy[50];
            Assert.Equal(50, i);

            i = (int)proxy[5];
            Assert.Equal(5, i);
        }

        [Fact]
        public void EnumerableIndexOutOfRange()
        {
            var numbers = Enumerable.Range(0, 100);
            var proxy = new ListProxy(numbers);

            Assert.Throws<ArgumentOutOfRangeException>(() => proxy[100]);
        }

        class IntCollection
            : ICollection
        {
            readonly List<int> ints;

            public IntCollection(IEnumerable<int> ints)
            {
                this.ints = ints.ToList();
            }

            public IEnumerator GetEnumerator()
            {
                return ints.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public int Count { get { return ints.Count; } }

            public object SyncRoot
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsSynchronized
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly { get { return true; } }
        }

        [Fact]
        public void CollectionIndexOutOfRange()
        {
            var numbers = new IntCollection(Enumerable.Range(0, 100));
            var proxy = new ListProxy(numbers);

            Assert.Throws<ArgumentOutOfRangeException>(() => proxy[100]);
        }

        [Fact]
        public void ListIndexOutOfRange()
        {
            var numbers = Enumerable.Range(0, 100).ToList();
            var proxy = new ListProxy(numbers);

            Assert.Throws<ArgumentOutOfRangeException>(() => proxy[100]);
        }

        [Fact]
        public void CollectionChangedWhileEnumerating()
        {
            var c = new ObservableCollection<string> { "foo", "bar" };
            var p = new ListProxy(c);

            IEnumerator<object> e = p.GetEnumerator();
            Assert.True(e.MoveNext(), "Initial MoveNext() failed, test can't continue");

            c.Add("baz");

            Assert.Throws<InvalidOperationException>(() => e.MoveNext());
        }

        [Fact]
        public void SynchronizedCollectionAccess()
        {
            var collection = new ObservableCollection<string> { "foo" };
            var context = new object();

            var list = new ListProxy(collection);

            bool executed = false;
            BindingBase.EnableCollectionSynchronization(collection, context, (enumerable, o, method, access) =>
            {
                executed = true;
                Assert.Same(collection, enumerable);
                Assert.Same(context, o);
                Assert.NotNull(method);
                Assert.False(access);

                lock (enumerable)
                    method();
            });

            object value = list[0];

            Assert.True(executed, "Callback was not executed");
        }

        [Fact]
        public Task SynchronizedCollectionAdd() => DispatcherTest.Run(() =>
        {
            bool invoked = false;

            DispatcherProviderStubOptions.IsInvokeRequired = () => true;
            DispatcherProviderStubOptions.InvokeOnMainThread = action =>
            {
                invoked = true;
                action();
            };

            var collection = new ObservableCollection<string> { "foo" };
            var context = new object();

            var list = new ListProxy(collection);

            Assert.False(invoked, "An invoke shouldn't be executed just setting up ListProxy");

            bool executed = false;
            BindingBase.EnableCollectionSynchronization(collection, context, (enumerable, o, method, access) =>
            {
                executed = true;
                Assert.Same(collection, enumerable);
                Assert.Same(context, o);
                Assert.NotNull(method);
                Assert.False(access);

                lock (enumerable)
                    method();
            });

            var mre = new ManualResetEvent(false);

            Task.Factory.StartNew(() =>
            {
                DispatcherProviderStubOptions.IsInvokeRequired = () => true;
                DispatcherProviderStubOptions.InvokeOnMainThread = action =>
                {
                    invoked = true;
                    action();
                };

                lock (collection)
                    collection.Add("foo");

                mre.Set();
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

            mre.WaitOne(5000);

            Assert.True(executed, "Callback was not executed");
            Assert.True(invoked, "Callback was not executed on the UI thread");
        });

        [Fact]
        public void ClearEnumerable()
        {
            var proxy = new ListProxy(Enumerable.Range(0, 100));
            var enumerator = proxy.GetEnumerator();
            enumerator.MoveNext();
            enumerator.MoveNext();

            proxy.Clear();

            Assert.Equal(100, proxy.Count);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Fact]
        public void ClearCollection()
        {
            var proxy = new ListProxy(new IntCollection(Enumerable.Range(0, 100)));
            var enumerator = proxy.GetEnumerator();
            enumerator.MoveNext();
            enumerator.MoveNext();

            proxy.Clear();

            Assert.Equal(100, proxy.Count);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Fact]
        public void ClearList()
        {
            var proxy = new ListProxy(Enumerable.Range(0, 100).ToList());
            var enumerator = proxy.GetEnumerator();
            enumerator.MoveNext();
            enumerator.MoveNext();

            proxy.Clear();

            Assert.Equal(100, proxy.Count);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        }

        [Fact]
        public void IndexOfValueTypeNonList()
        {
            var proxy = new ListProxy(Enumerable.Range(0, 100));
            Assert.Equal(1, proxy.IndexOf(1));
        }

        [Fact]
        public void EnumeratorForEnumerable()
        {
            var proxy = new ListProxy(Enumerable.Range(0, 2));

            var enumerator = proxy.GetEnumerator();
            Assert.Null(enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(0, enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(1, enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void ProxyIsWeaklyHeldByINotifyCollectionChanged()
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();

            WeakReference weakProxy = null;

            int i = 0;
            Action create = null;
            create = () =>
            {
                if (i++ < 1024)
                {
                    create();
                    return;
                }

                var proxy = new ListProxy(collection);
                weakProxy = new WeakReference(proxy);
            };

            create();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(weakProxy.IsAlive);
        }

        [Fact]
        public void IEnumerableAddDoesNotReport0()
        {
            var custom = new CustomINCC();
            custom.Add("test");
            custom.Add("test2");

            var proxy = new ListProxy(custom);
            Assert.Equal(2, proxy.Count);

            custom.Add("testing");
            Assert.Equal(3, proxy.Count);
        }

        class CustomINCC : IEnumerable<string>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;
            List<string> Items = new List<string>();

            public void Add(string s)
            {
                Items.Add(s);
                if (CollectionChanged != null)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, s));
            }

            public IEnumerator<string> GetEnumerator()
            {
                return Items.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return Items.GetEnumerator();
            }
        }

        // Need a member to keep this reference around, otherwise it gets optimized
        // out early in Release mode during the WeakToWeak test
#pragma warning disable 0414 // Never accessed, it's just here to prevent GC
        ListProxy _proxyForWeakToWeakTest;
#pragma warning restore 0414

        [Fact(Skip = "https://github.com/dotnet/maui/issues/1524")]
        public void WeakToWeak()
        {
            WeakCollectionChangedList list = new WeakCollectionChangedList();
            _proxyForWeakToWeakTest = new ListProxy(list);

            Assert.True(list.AddObject(), "GC hasn't run");

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.True(list.AddObject(), "GC run, but proxy should still hold a reference");

            _proxyForWeakToWeakTest = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.False(list.AddObject(), "Proxy is gone and GC has run");
        }

        public class WeakCollectionChangedList : List<object>, INotifyCollectionChanged
        {
            List<WeakHandler> handlers = new List<WeakHandler>();

            public WeakCollectionChangedList()
            {

            }
            public event NotifyCollectionChangedEventHandler CollectionChanged
            {
                add { handlers.Add(new WeakHandler(this, value)); }
                remove { throw new NotImplementedException(); }
            }


            public bool AddObject()
            {
                bool invoked = false;
                var me = new object();
                Console.WriteLine($"Handler count is {handlers.Count}");
                foreach (var handler in handlers.ToList())
                {
                    if (handler.IsActive)
                    {
                        invoked = true;
                        handler.Handler.DynamicInvoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, me));
                    }
                    else
                    {
                        Console.WriteLine($"Handler is inactive");
                        handlers.Remove(handler);
                    }
                }

                return invoked;
            }

            class WeakHandler
            {
                WeakReference source;
                WeakReference originalHandler;

                public bool IsActive
                {
                    get { return this.source != null && this.source.IsAlive && this.originalHandler != null && this.originalHandler.IsAlive; }
                }

                public NotifyCollectionChangedEventHandler Handler
                {
                    get
                    {
                        if (this.originalHandler == null)
                        {
                            return default(NotifyCollectionChangedEventHandler);
                        }
                        else
                        {
                            return (NotifyCollectionChangedEventHandler)this.originalHandler.Target;
                        }
                    }
                }

                public WeakHandler(object source, NotifyCollectionChangedEventHandler originalHandler)
                {
                    this.source = new WeakReference(source);
                    this.originalHandler = new WeakReference(originalHandler);
                }
            }
        }

        /// <summary>
        /// Tests IndexOf method when the proxied enumerable implements IList.
        /// Should delegate directly to the underlying IList.IndexOf method.
        /// </summary>
        [Fact]
        public void IndexOf_WithIListImplementation_ReturnsCorrectIndex()
        {
            // Arrange
            var list = new List<object> { "first", "second", "third" };
            var proxy = new ListProxy(list);

            // Act & Assert
            Assert.Equal(0, proxy.IndexOf("first"));
            Assert.Equal(1, proxy.IndexOf("second"));
            Assert.Equal(2, proxy.IndexOf("third"));
        }

        /// <summary>
        /// Tests IndexOf method when searching for an item that doesn't exist in an IList.
        /// Should return -1 via delegation to underlying IList.IndexOf method.
        /// </summary>
        [Fact]
        public void IndexOf_WithIListImplementation_ItemNotFound_ReturnsMinusOne()
        {
            // Arrange
            var list = new List<object> { "first", "second", "third" };
            var proxy = new ListProxy(list);

            // Act & Assert
            Assert.Equal(-1, proxy.IndexOf("nonexistent"));
            Assert.Equal(-1, proxy.IndexOf(null));
        }

        /// <summary>
        /// Tests IndexOf method with null parameter when using IList implementation.
        /// Should handle null search gracefully via delegation to underlying IList.
        /// </summary>
        [Fact]
        public void IndexOf_WithIListImplementation_NullItem_HandledCorrectly()
        {
            // Arrange
            var list = new List<object> { "first", null, "third" };
            var proxy = new ListProxy(list);

            // Act & Assert
            Assert.Equal(1, proxy.IndexOf(null));
        }

        /// <summary>
        /// Tests IndexOf method when the proxied enumerable doesn't implement IList and item is not found.
        /// Should iterate through windowed items and return -1 when not found.
        /// </summary>
        [Fact]
        public void IndexOf_WithEnumerableItemNotFound_ReturnsMinusOne()
        {
            // Arrange
            var enumerable = Enumerable.Range(1, 10).Cast<object>();
            var proxy = new ListProxy(enumerable);

            // Act & Assert
            Assert.Equal(-1, proxy.IndexOf(0));
            Assert.Equal(-1, proxy.IndexOf(11));
            Assert.Equal(-1, proxy.IndexOf("string"));
            Assert.Equal(-1, proxy.IndexOf(null));
        }

        /// <summary>
        /// Tests IndexOf method with empty enumerable that doesn't implement IList.
        /// Should handle empty collections gracefully and return -1.
        /// </summary>
        [Fact]
        public void IndexOf_WithEmptyEnumerable_ReturnsMinusOne()
        {
            // Arrange
            var emptyEnumerable = Enumerable.Empty<object>();
            var proxy = new ListProxy(emptyEnumerable);

            // Act & Assert
            Assert.Equal(-1, proxy.IndexOf("anything"));
            Assert.Equal(-1, proxy.IndexOf(null));
            Assert.Equal(-1, proxy.IndexOf(42));
        }

        /// <summary>
        /// Tests IndexOf method with various edge case values when using enumerable.
        /// Should handle special values correctly through the windowing mechanism.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        public void IndexOf_WithEnumerableSpecialValues_ReturnsCorrectIndex(int searchValue)
        {
            // Arrange
            var values = new object[] { int.MinValue, int.MaxValue, 0, -1, "test" };
            var proxy = new ListProxy(values);

            // Act & Assert
            int expectedIndex = Array.IndexOf(values, searchValue);
            Assert.Equal(expectedIndex, proxy.IndexOf(searchValue));
        }

        /// <summary>
        /// Tests IndexOf method with floating-point special values when using enumerable.
        /// Should handle NaN, positive/negative infinity correctly.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void IndexOf_WithFloatingPointSpecialValues_ReturnsCorrectIndex(double searchValue)
        {
            // Arrange
            var values = new object[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, 1.0 };
            var proxy = new ListProxy(values);

            // Act & Assert
            int expectedIndex = -1;
            for (int i = 0; i < values.Length; i++)
            {
                if (Equals(values[i], searchValue))
                {
                    expectedIndex = i;
                    break;
                }
            }
            Assert.Equal(expectedIndex, proxy.IndexOf(searchValue));
        }

        /// <summary>
        /// Tests IndexOf method with string edge cases when using enumerable.
        /// Should handle null, empty, whitespace, and special character strings.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\n\r")]
        [InlineData("\0")]
        public void IndexOf_WithStringEdgeCases_ReturnsCorrectIndex(string searchValue)
        {
            // Arrange
            var values = new object[] { null, "", " ", "\t\n\r", "\0", "normal" };
            var proxy = new ListProxy(values);

            // Act & Assert
            int expectedIndex = Array.IndexOf(values, searchValue);
            Assert.Equal(expectedIndex, proxy.IndexOf(searchValue));
        }

        /// <summary>
        /// Tests IndexOf method with reference type equality comparisons.
        /// Should use Equals method for comparison, not reference equality.
        /// </summary>
        [Fact]
        public void IndexOf_WithReferenceTypes_UsesEqualsComparison()
        {
            // Arrange
            var str1 = new string("test".ToCharArray());
            var str2 = new string("test".ToCharArray());
            var values = new object[] { str1, "other" };
            var proxy = new ListProxy(values);

            // Act & Assert
            Assert.Equal(0, proxy.IndexOf(str2)); // Should find via Equals, not reference equality
        }

        /// <summary>
        /// Tests that Count returns 0 when the enumerable is empty and not a collection.
        /// This test specifically targets the uncovered return 0 path in the Count property
        /// where _collection is null and _indexesCounted remains null after EnsureWindowCreated().
        /// </summary>
        [Fact]
        public void Count_EmptyEnumerableNotCollection_ReturnsZero()
        {
            // Arrange
            var emptyEnumerable = Enumerable.Empty<int>();
            var proxy = new ListProxy(emptyEnumerable);

            // Act
            var count = proxy.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests that Count returns 0 when using a custom empty enumerable that is neither
        /// ICollection nor IReadOnlyCollection of object. This ensures the return 0 path
        /// is covered for various enumerable types.
        /// </summary>
        [Fact]
        public void Count_CustomEmptyEnumerable_ReturnsZero()
        {
            // Arrange
            var customEmptyEnumerable = new CustomEmptyEnumerable();
            var proxy = new ListProxy(customEmptyEnumerable);

            // Act
            var count = proxy.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Tests Count with an enumerable that becomes empty after filtering.
        /// This tests the return 0 path with a LINQ-generated enumerable.
        /// </summary>
        [Fact]
        public void Count_FilteredEmptyEnumerable_ReturnsZero()
        {
            // Arrange
            var filteredEmpty = Enumerable.Range(1, 10).Where(x => x > 20);
            var proxy = new ListProxy(filteredEmpty);

            // Act
            var count = proxy.Count;

            // Assert
            Assert.Equal(0, count);
        }

        /// <summary>
        /// Helper class that implements IEnumerable but not ICollection or IReadOnlyCollection
        /// and returns no items when enumerated.
        /// </summary>
        private class CustomEmptyEnumerable : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                yield break;
            }
        }

        /// <summary>
        /// Tests that Reset method correctly resets the enumerator index to 0 and Current to null on a fresh enumerator.
        /// Input conditions: Fresh ProxyEnumerator that has not been moved.
        /// Expected result: _index set to 0, Current set to null.
        /// </summary>
        [Fact]
        public void Reset_FreshEnumerator_ResetsIndexAndCurrent()
        {
            // Arrange
            var enumerable = new List<object> { "item1", "item2", "item3" };
            var listProxy = new ListProxy(enumerable);
            var enumerator = listProxy.GetEnumerator();

            // Act
            enumerator.Reset();

            // Assert
            Assert.Null(enumerator.Current);
        }

        /// <summary>
        /// Tests that Reset method correctly resets the enumerator after it has been advanced.
        /// Input conditions: ProxyEnumerator that has been moved to a specific position.
        /// Expected result: _index set to 0, Current set to null, and enumerator can enumerate from beginning again.
        /// </summary>
        [Fact]
        public void Reset_AfterMoveNext_ResetsIndexAndCurrent()
        {
            // Arrange
            var enumerable = new List<object> { "item1", "item2", "item3" };
            var listProxy = new ListProxy(enumerable);
            var enumerator = listProxy.GetEnumerator();

            enumerator.MoveNext();
            enumerator.MoveNext();
            var currentBeforeReset = enumerator.Current;

            // Act
            enumerator.Reset();

            // Assert
            Assert.Null(enumerator.Current);
            Assert.NotNull(currentBeforeReset); // Verify we had moved before reset

            // Verify we can enumerate from beginning again
            Assert.True(enumerator.MoveNext());
            Assert.Equal("item1", enumerator.Current);
        }

        /// <summary>
        /// Tests that Reset method can be called multiple times safely.
        /// Input conditions: ProxyEnumerator with multiple consecutive Reset calls.
        /// Expected result: Each Reset call sets _index to 0 and Current to null.
        /// </summary>
        [Fact]
        public void Reset_MultipleConsecutiveCalls_AlwaysResetsCorrectly()
        {
            // Arrange
            var enumerable = new List<object> { "item1", "item2" };
            var listProxy = new ListProxy(enumerable);
            var enumerator = listProxy.GetEnumerator();

            // Act & Assert - Multiple reset calls
            enumerator.Reset();
            Assert.Null(enumerator.Current);

            enumerator.Reset();
            Assert.Null(enumerator.Current);

            enumerator.Reset();
            Assert.Null(enumerator.Current);

            // Verify functionality still works after multiple resets
            Assert.True(enumerator.MoveNext());
            Assert.Equal("item1", enumerator.Current);
        }

        /// <summary>
        /// Tests that Reset method works correctly after enumerator has reached the end.
        /// Input conditions: ProxyEnumerator that has been fully enumerated to the end.
        /// Expected result: _index set to 0, Current set to null, and enumerator can restart enumeration.
        /// </summary>
        [Fact]
        public void Reset_AfterFullEnumeration_ResetsToBeginning()
        {
            // Arrange
            var enumerable = new List<object> { "item1", "item2" };
            var listProxy = new ListProxy(enumerable);
            var enumerator = listProxy.GetEnumerator();

            // Enumerate to end
            while (enumerator.MoveNext()) { }

            // Act
            enumerator.Reset();

            // Assert
            Assert.Null(enumerator.Current);

            // Verify we can enumerate from beginning again
            Assert.True(enumerator.MoveNext());
            Assert.Equal("item1", enumerator.Current);
            Assert.True(enumerator.MoveNext());
            Assert.Equal("item2", enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests that Reset method works with empty collection.
        /// Input conditions: ProxyEnumerator for empty collection.
        /// Expected result: _index set to 0, Current set to null.
        /// </summary>
        [Fact]
        public void Reset_EmptyCollection_ResetsCorrectly()
        {
            // Arrange
            var enumerable = new List<object>();
            var listProxy = new ListProxy(enumerable);
            var enumerator = listProxy.GetEnumerator();

            // Act
            enumerator.Reset();

            // Assert
            Assert.Null(enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }
    }


    public partial class ListProxyContainsTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Contains returns true when the item exists in a List-based collection.
        /// </summary>
        [Fact]
        public void Contains_WithListAndItemExists_ReturnsTrue()
        {
            // Arrange
            var list = new List<object> { "item1", "item2", "item3" };
            var proxy = new ListProxy(list);

            // Act
            var result = proxy.Contains("item2");

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in a List-based collection.
        /// </summary>
        [Fact]
        public void Contains_WithListAndItemNotExists_ReturnsFalse()
        {
            // Arrange
            var list = new List<object> { "item1", "item2", "item3" };
            var proxy = new ListProxy(list);

            // Act
            var result = proxy.Contains("nonexistent");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains handles null items correctly in a List-based collection.
        /// </summary>
        [Fact]
        public void Contains_WithListAndNullItem_ReturnsTrue()
        {
            // Arrange
            var list = new List<object> { "item1", null, "item3" };
            var proxy = new ListProxy(list);

            // Act
            var result = proxy.Contains(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false for null item when not present in List-based collection.
        /// </summary>
        [Fact]
        public void Contains_WithListAndNullItemNotPresent_ReturnsFalse()
        {
            // Arrange
            var list = new List<object> { "item1", "item2", "item3" };
            var proxy = new ListProxy(list);

            // Act
            var result = proxy.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns true when the item exists in an Enumerable-based collection.
        /// </summary>
        [Fact]
        public void Contains_WithEnumerableAndItemExists_ReturnsTrue()
        {
            // Arrange
            var enumerable = Enumerable.Range(1, 5).Cast<object>();
            var proxy = new ListProxy(enumerable);

            // Act
            var result = proxy.Contains(3);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the item does not exist in an Enumerable-based collection.
        /// </summary>
        [Fact]
        public void Contains_WithEnumerableAndItemNotExists_ReturnsFalse()
        {
            // Arrange
            var enumerable = Enumerable.Range(1, 5).Cast<object>();
            var proxy = new ListProxy(enumerable);

            // Act
            var result = proxy.Contains(10);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when searching in an empty enumerable.
        /// </summary>
        [Fact]
        public void Contains_WithEmptyEnumerable_ReturnsFalse()
        {
            // Arrange
            var enumerable = Enumerable.Empty<object>();
            var proxy = new ListProxy(enumerable);

            // Act
            var result = proxy.Contains("anything");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when searching for null in an empty enumerable.
        /// </summary>
        [Fact]
        public void Contains_WithEmptyEnumerableAndNullItem_ReturnsFalse()
        {
            // Arrange
            var enumerable = Enumerable.Empty<object>();
            var proxy = new ListProxy(enumerable);

            // Act
            var result = proxy.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with different object types in enumerable.
        /// </summary>
        [Fact]
        public void Contains_WithEnumerableAndDifferentTypes_ReturnsCorrectResult()
        {
            // Arrange
            var enumerable = new object[] { "string", 42, DateTime.Now, null };
            var proxy = new ListProxy(enumerable);

            // Act & Assert
            Assert.True(proxy.Contains("string"));
            Assert.True(proxy.Contains(42));
            Assert.True(proxy.Contains(null));
            Assert.False(proxy.Contains("notfound"));
            Assert.False(proxy.Contains(999));
        }

        /// <summary>
        /// Tests that Contains works with IReadOnlyList which gets wrapped as IList.
        /// </summary>
        [Fact]
        public void Contains_WithReadOnlyList_ReturnsCorrectResult()
        {
            // Arrange
            var readOnlyList = new ReadOnlyCollection<object>(new List<object> { "a", "b", "c" });
            var proxy = new ListProxy(readOnlyList);

            // Act & Assert
            Assert.True(proxy.Contains("b"));
            Assert.False(proxy.Contains("z"));
        }

        /// <summary>
        /// Tests that Contains handles windowed enumerable correctly when item is in the window.
        /// </summary>
        [Fact]
        public void Contains_WithWindowedEnumerableAndItemInWindow_ReturnsTrue()
        {
            // Arrange
            var enumerable = Enumerable.Range(1, 100).Cast<object>();
            var proxy = new ListProxy(enumerable, windowSize: 10);

            // Force window creation by accessing an item
            var _ = proxy[0];

            // Act
            var result = proxy.Contains(5); // Should be in the first window

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when item is outside the current window in windowed enumerable.
        /// </summary>
        [Fact]
        public void Contains_WithWindowedEnumerableAndItemOutsideWindow_ReturnsFalse()
        {
            // Arrange
            var enumerable = Enumerable.Range(1, 100).Cast<object>();
            var proxy = new ListProxy(enumerable, windowSize: 5);

            // Force window creation by accessing an item
            var _ = proxy[0];

            // Act
            var result = proxy.Contains(50); // Should be outside the first window

            // Assert
            Assert.False(result);
        }
    }
}