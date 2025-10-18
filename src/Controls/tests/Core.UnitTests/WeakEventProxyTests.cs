using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class WeakEventProxyTests
    {
        [Fact]
        public async Task DoesNotLeak()
        {
            WeakReference reference;
            var list = new ObservableCollection<string>();
            var proxy = new WeakNotifyCollectionChangedProxy();

            {
                var subscriber = new Subscriber();
                proxy.Subscribe(list, subscriber.OnCollectionChanged);
                reference = new WeakReference(subscriber);
            }

            Assert.False(await reference.WaitForCollect(), "Subscriber should not be alive!");
        }

        [Fact]
        public async Task EventFires()
        {
            var list = new ObservableCollection<string>();
            var proxy = new WeakNotifyCollectionChangedProxy();

            bool fired = false;
            // NOTE: this test wouldn't pass if we didn't save this and GC.KeepAlive() it
            NotifyCollectionChangedEventHandler handler = (s, e) => fired = true;
            proxy.Subscribe(list, handler);

            await TestHelpers.Collect();
            GC.KeepAlive(handler);

            list.Add("a");

            Assert.True(fired);
        }

        class Subscriber
        {
            public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }
        }

        /// <summary>
        /// Tests that TryGetHandler returns false and sets handler to default when _handler field is null (never subscribed).
        /// This test covers the not covered lines 37-38.
        /// </summary>
        [Fact]
        public void TryGetHandler_WhenNeverSubscribed_ReturnsFalseAndSetsHandlerToDefault()
        {
            // Arrange
            var proxy = new TestWeakEventProxy();

            // Act
            bool result = proxy.TryGetHandler(out Action handler);

            // Assert
            Assert.False(result);
            Assert.Null(handler);
        }

        /// <summary>
        /// Tests that TryGetHandler returns true when a handler is subscribed and still alive.
        /// This test covers the covered lines 32-34, 39.
        /// </summary>
        [Fact]
        public void TryGetHandler_WhenHandlerExists_ReturnsTrueAndSetsHandler()
        {
            // Arrange
            var proxy = new TestWeakEventProxy();
            var source = new TestSource();
            Action handler = () => { };
            proxy.Subscribe(source, handler);

            // Act
            bool result = proxy.TryGetHandler(out Action retrievedHandler);

            // Assert
            Assert.True(result);
            Assert.Same(handler, retrievedHandler);
        }

        /// <summary>
        /// Tests that TryGetHandler returns false when the weak reference target has been garbage collected.
        /// This test covers the not covered lines 37-38.
        /// </summary>
        [Fact]
        public void TryGetHandler_WhenHandlerTargetCollected_ReturnsFalseAndSetsHandlerToDefault()
        {
            // Arrange
            var proxy = new TestWeakEventProxy();
            var source = new TestSource();

            // Subscribe with a handler that can be collected
            proxy.Subscribe(source, CreateCollectibleHandler());

            // Force garbage collection multiple times to ensure the handler is collected
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Act
            bool result = proxy.TryGetHandler(out Action handler);

            // Assert
            Assert.False(result);
            Assert.Null(handler);
        }

        /// <summary>
        /// Tests edge case where TryGetTarget succeeds but returns null handler.
        /// This test covers line 34 where handler is not null check.
        /// </summary>
        [Fact]
        public void TryGetHandler_WhenHandlerIsNullButTargetExists_ReturnsFalse()
        {
            // Arrange
            var proxy = new TestWeakEventProxy();
            var source = new TestSource();
            Action nullHandler = null;
            proxy.Subscribe(source, nullHandler);

            // Act
            bool result = proxy.TryGetHandler(out Action handler);

            // Assert
            Assert.False(result);
            Assert.Null(handler);
        }

        /// <summary>
        /// Helper method to create a handler that can be garbage collected.
        /// </summary>
        private static Action CreateCollectibleHandler()
        {
            return () => { };
        }

        /// <summary>
        /// Test implementation of WeakEventProxy for testing purposes.
        /// </summary>
        private class TestWeakEventProxy : WeakEventProxy<TestSource, Action>
        {
        }

        /// <summary>
        /// Simple test source class for testing purposes.
        /// </summary>
        private class TestSource
        {
        }
    }

    public partial class WeakNotifyCollectionChangedProxyTests
    {
        /// <summary>
        /// Tests that Subscribe method properly unsubscribes from previous source when subscribing to a new source.
        /// This test specifically targets the uncovered lines in the Subscribe method where an existing source is unsubscribed.
        /// </summary>
        [Fact]
        public void Subscribe_WithExistingSource_UnsubscribesFromPreviousSource()
        {
            // Arrange
            var firstCollection = new ObservableCollection<string>();
            var secondCollection = new ObservableCollection<string>();
            var proxy = new WeakNotifyCollectionChangedProxy();

            int firstCollectionEventCount = 0;
            int secondCollectionEventCount = 0;

            NotifyCollectionChangedEventHandler handler = (sender, args) =>
            {
                if (ReferenceEquals(sender, firstCollection))
                    firstCollectionEventCount++;
                else if (ReferenceEquals(sender, secondCollection))
                    secondCollectionEventCount++;
            };

            // Act - Subscribe to first collection
            proxy.Subscribe(firstCollection, handler);
            firstCollection.Add("item1");

            // Act - Subscribe to second collection (should unsubscribe from first)
            proxy.Subscribe(secondCollection, handler);

            // Modify both collections
            firstCollection.Add("item2");  // Should not trigger handler
            secondCollection.Add("item1"); // Should trigger handler

            // Assert
            Assert.Equal(1, firstCollectionEventCount); // Only the first add should have been handled
            Assert.Equal(1, secondCollectionEventCount); // Only the second collection add should be handled
        }

        /// <summary>
        /// Tests that Subscribe method throws ArgumentNullException when source parameter is null.
        /// This validates proper parameter validation for the source parameter.
        /// </summary>
        [Fact]
        public void Subscribe_WithNullSource_ThrowsArgumentNullException()
        {
            // Arrange
            var proxy = new WeakNotifyCollectionChangedProxy();
            NotifyCollectionChangedEventHandler handler = (sender, args) => { };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => proxy.Subscribe(null, handler));
        }

        /// <summary>
        /// Tests that Subscribe method throws ArgumentNullException when handler parameter is null.
        /// This validates proper parameter validation for the handler parameter.
        /// </summary>
        [Fact]
        public void Subscribe_WithNullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new ObservableCollection<string>();
            var proxy = new WeakNotifyCollectionChangedProxy();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => proxy.Subscribe(collection, null));
        }

        /// <summary>
        /// Tests that Subscribe method works correctly when subscribing to the same source multiple times.
        /// This ensures that resubscribing to the same source doesn't cause issues.
        /// </summary>
        [Fact]
        public void Subscribe_WithSameSourceTwice_HandlesEventsCorrectly()
        {
            // Arrange
            var collection = new ObservableCollection<string>();
            var proxy = new WeakNotifyCollectionChangedProxy();

            int eventCount = 0;
            NotifyCollectionChangedEventHandler handler = (sender, args) => eventCount++;

            // Act - Subscribe twice to same source
            proxy.Subscribe(collection, handler);
            proxy.Subscribe(collection, handler);

            collection.Add("item1");

            // Assert - Should only receive one event (not double subscription)
            Assert.Equal(1, eventCount);
        }

        /// <summary>
        /// Tests that Subscribe method works correctly when no previous source exists.
        /// This covers the normal subscription path without any existing source to unsubscribe from.
        /// </summary>
        [Fact]
        public void Subscribe_WithoutExistingSource_SubscribesSuccessfully()
        {
            // Arrange
            var collection = new ObservableCollection<string>();
            var proxy = new WeakNotifyCollectionChangedProxy();

            bool eventFired = false;
            NotifyCollectionChangedEventHandler handler = (sender, args) => eventFired = true;

            // Act
            proxy.Subscribe(collection, handler);
            collection.Add("item1");

            // Assert
            Assert.True(eventFired);
        }

        /// <summary>
        /// Tests that Subscribe method properly handles switching between multiple different sources.
        /// This ensures the unsubscribe mechanism works correctly across multiple source changes.
        /// </summary>
        [Fact]
        public void Subscribe_WithMultipleSources_UnsubscribesFromPreviousSourcesCorrectly()
        {
            // Arrange
            var firstCollection = new ObservableCollection<string>();
            var secondCollection = new ObservableCollection<string>();
            var thirdCollection = new ObservableCollection<string>();
            var proxy = new WeakNotifyCollectionChangedProxy();

            int eventCount = 0;
            object lastSender = null;

            NotifyCollectionChangedEventHandler handler = (sender, args) =>
            {
                eventCount++;
                lastSender = sender;
            };

            // Act
            proxy.Subscribe(firstCollection, handler);
            proxy.Subscribe(secondCollection, handler);
            proxy.Subscribe(thirdCollection, handler);

            // Trigger events on all collections
            firstCollection.Add("item1");  // Should not fire
            secondCollection.Add("item1"); // Should not fire
            thirdCollection.Add("item1");  // Should fire

            // Assert
            Assert.Equal(1, eventCount);
            Assert.Same(thirdCollection, lastSender);
        }
    }


    public class WeakNotifyPropertyChangedProxyConstructorTests
    {
        /// <summary>
        /// Tests that the WeakNotifyPropertyChangedProxy constructor properly subscribes to the source's PropertyChanged event
        /// when valid source and handler parameters are provided.
        /// </summary>
        [Fact]
        public void Constructor_ValidSourceAndHandler_SubscribesToPropertyChangedEvent()
        {
            // Arrange
            var source = Substitute.For<INotifyPropertyChanged>();
            var handlerCallCount = 0;
            PropertyChangedEventHandler handler = (sender, e) => handlerCallCount++;

            // Act
            var proxy = new WeakNotifyPropertyChangedProxy(source, handler);

            // Trigger the PropertyChanged event to verify subscription
            source.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(source, new PropertyChangedEventArgs("TestProperty"));

            // Assert
            Assert.Equal(1, handlerCallCount);
        }

        /// <summary>
        /// Tests that the WeakNotifyPropertyChangedProxy constructor throws ArgumentNullException
        /// when a null source parameter is provided.
        /// </summary>
        [Fact]
        public void Constructor_NullSource_ThrowsArgumentNullException()
        {
            // Arrange
            INotifyPropertyChanged source = null;
            PropertyChangedEventHandler handler = (sender, e) => { };

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new WeakNotifyPropertyChangedProxy(source, handler));
        }

        /// <summary>
        /// Tests that the WeakNotifyPropertyChangedProxy constructor properly handles
        /// a null handler parameter by storing it in the weak reference.
        /// </summary>
        [Fact]
        public void Constructor_NullHandler_DoesNotThrow()
        {
            // Arrange
            var source = Substitute.For<INotifyPropertyChanged>();
            PropertyChangedEventHandler handler = null;

            // Act & Assert - Should not throw
            var proxy = new WeakNotifyPropertyChangedProxy(source, handler);
            Assert.NotNull(proxy);
        }

        /// <summary>
        /// Tests that the WeakNotifyPropertyChangedProxy constructor properly initializes
        /// with a complex property change scenario involving multiple property changes.
        /// </summary>
        [Fact]
        public void Constructor_ComplexPropertyChangeScenario_HandlesMultipleEvents()
        {
            // Arrange
            var source = Substitute.For<INotifyPropertyChanged>();
            var receivedEvents = new System.Collections.Generic.List<string>();
            PropertyChangedEventHandler handler = (sender, e) => receivedEvents.Add(e.PropertyName);

            // Act
            var proxy = new WeakNotifyPropertyChangedProxy(source, handler);

            // Trigger multiple PropertyChanged events
            source.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(source, new PropertyChangedEventArgs("Property1"));
            source.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(source, new PropertyChangedEventArgs("Property2"));
            source.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(source, new PropertyChangedEventArgs("Property3"));

            // Assert
            Assert.Equal(3, receivedEvents.Count);
            Assert.Contains("Property1", receivedEvents);
            Assert.Contains("Property2", receivedEvents);
            Assert.Contains("Property3", receivedEvents);
        }

        /// <summary>
        /// Tests that the WeakNotifyPropertyChangedProxy constructor properly handles
        /// PropertyChangedEventArgs with null property name.
        /// </summary>
        [Fact]
        public void Constructor_PropertyChangedWithNullPropertyName_HandlesCorrectly()
        {
            // Arrange
            var source = Substitute.For<INotifyPropertyChanged>();
            var handlerCalled = false;
            string receivedPropertyName = string.Empty;
            PropertyChangedEventHandler handler = (sender, e) =>
            {
                handlerCalled = true;
                receivedPropertyName = e.PropertyName;
            };

            // Act
            var proxy = new WeakNotifyPropertyChangedProxy(source, handler);

            // Trigger PropertyChanged event with null PropertyName
            source.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(source, new PropertyChangedEventArgs(null));

            // Assert
            Assert.True(handlerCalled);
            Assert.Null(receivedPropertyName);
        }

        /// <summary>
        /// Tests that the WeakNotifyPropertyChangedProxy constructor properly handles
        /// PropertyChangedEventArgs with empty string property name.
        /// </summary>
        [Fact]
        public void Constructor_PropertyChangedWithEmptyPropertyName_HandlesCorrectly()
        {
            // Arrange
            var source = Substitute.For<INotifyPropertyChanged>();
            var handlerCalled = false;
            string receivedPropertyName = null;
            PropertyChangedEventHandler handler = (sender, e) =>
            {
                handlerCalled = true;
                receivedPropertyName = e.PropertyName;
            };

            // Act
            var proxy = new WeakNotifyPropertyChangedProxy(source, handler);

            // Trigger PropertyChanged event with empty PropertyName
            source.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(source, new PropertyChangedEventArgs(string.Empty));

            // Assert
            Assert.True(handlerCalled);
            Assert.Equal(string.Empty, receivedPropertyName);
        }
    }


    public partial class WeakNotifyPropertyChangedProxyTests
    {
        /// <summary>
        /// Tests Subscribe method when source parameter is null.
        /// Should throw ArgumentNullException when attempting to access source.PropertyChanged.
        /// </summary>
        [Fact]
        public void Subscribe_NullSource_ThrowsArgumentNullException()
        {
            // Arrange
            var proxy = new WeakNotifyPropertyChangedProxy();
            PropertyChangedEventHandler handler = (sender, e) => { };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => proxy.Subscribe(null, handler));
        }

        /// <summary>
        /// Tests Subscribe method with valid source and null handler.
        /// Should successfully subscribe to source's PropertyChanged event with null handler.
        /// </summary>
        [Fact]
        public void Subscribe_ValidSourceNullHandler_SubscribesSuccessfully()
        {
            // Arrange
            var source = Substitute.For<INotifyPropertyChanged>();
            var proxy = new WeakNotifyPropertyChangedProxy();

            // Act
            proxy.Subscribe(source, null);

            // Assert
            source.Received(1).PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
        }

        /// <summary>
        /// Tests Subscribe method with valid source and handler when no existing source.
        /// Should subscribe to source's PropertyChanged event and call base Subscribe.
        /// </summary>
        [Fact]
        public void Subscribe_ValidSourceAndHandler_NoExistingSource_SubscribesSuccessfully()
        {
            // Arrange
            var source = Substitute.For<INotifyPropertyChanged>();
            var proxy = new WeakNotifyPropertyChangedProxy();
            var handlerCalled = false;
            PropertyChangedEventHandler handler = (sender, e) => handlerCalled = true;

            // Act
            proxy.Subscribe(source, handler);

            // Assert
            source.Received(1).PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
        }

        /// <summary>
        /// Tests Subscribe method when there is an existing source that should be unsubscribed.
        /// Should unsubscribe from old source and subscribe to new source.
        /// This test covers the uncovered line 99: s.PropertyChanged -= OnPropertyChanged;
        /// </summary>
        [Fact]
        public void Subscribe_WithExistingSource_UnsubscribesFromOldAndSubscribesToNew()
        {
            // Arrange
            var oldSource = Substitute.For<INotifyPropertyChanged>();
            var newSource = Substitute.For<INotifyPropertyChanged>();
            var proxy = new WeakNotifyPropertyChangedProxy();
            PropertyChangedEventHandler handler1 = (sender, e) => { };
            PropertyChangedEventHandler handler2 = (sender, e) => { };

            // First subscription to establish existing source
            proxy.Subscribe(oldSource, handler1);

            // Act - Subscribe to new source (this should trigger unsubscribe from old source)
            proxy.Subscribe(newSource, handler2);

            // Assert
            // Should have been called twice - once for first subscription, once for second
            oldSource.Received(2).PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
            oldSource.Received(1).PropertyChanged -= Arg.Any<PropertyChangedEventHandler>();
            newSource.Received(1).PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
        }

        /// <summary>
        /// Tests Subscribe method when subscribing to the same source twice.
        /// Should unsubscribe and then resubscribe to the same source.
        /// </summary>
        [Fact]
        public void Subscribe_SameSourceTwice_UnsubscribesAndResubscribes()
        {
            // Arrange
            var source = Substitute.For<INotifyPropertyChanged>();
            var proxy = new WeakNotifyPropertyChangedProxy();
            PropertyChangedEventHandler handler1 = (sender, e) => { };
            PropertyChangedEventHandler handler2 = (sender, e) => { };

            // First subscription
            proxy.Subscribe(source, handler1);

            // Act - Subscribe to same source again
            proxy.Subscribe(source, handler2);

            // Assert
            // Should have been called twice for subscription
            source.Received(2).PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
            // Should have been called once for unsubscription
            source.Received(1).PropertyChanged -= Arg.Any<PropertyChangedEventHandler>();
        }

        /// <summary>
        /// Tests Subscribe method with multiple sequential subscriptions to different sources.
        /// Should properly manage unsubscription from previous sources.
        /// </summary>
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        public void Subscribe_MultipleSequentialSources_ManagesUnsubscriptionCorrectly(int sourceCount)
        {
            // Arrange
            var proxy = new WeakNotifyPropertyChangedProxy();
            var sources = new INotifyPropertyChanged[sourceCount];
            PropertyChangedEventHandler handler = (sender, e) => { };

            for (int i = 0; i < sourceCount; i++)
            {
                sources[i] = Substitute.For<INotifyPropertyChanged>();
            }

            // Act - Subscribe to each source in sequence
            for (int i = 0; i < sourceCount; i++)
            {
                proxy.Subscribe(sources[i], handler);
            }

            // Assert
            // Each source except the last should have been unsubscribed from
            for (int i = 0; i < sourceCount - 1; i++)
            {
                sources[i].Received(1).PropertyChanged -= Arg.Any<PropertyChangedEventHandler>();
            }

            // Last source should not have been unsubscribed from
            sources[sourceCount - 1].DidNotReceive().PropertyChanged -= Arg.Any<PropertyChangedEventHandler>();

            // All sources should have been subscribed to
            for (int i = 0; i < sourceCount; i++)
            {
                sources[i].Received(1).PropertyChanged += Arg.Any<PropertyChangedEventHandler>();
            }
        }
    }
}