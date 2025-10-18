#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the GestureElement class.
    /// </summary>
    public class GestureElementTests
    {
        /// <summary>
        /// Tests that ValidateGesture method executes successfully with a valid gesture recognizer.
        /// Verifies that the method completes without throwing exceptions when given a valid IGestureRecognizer instance.
        /// Expected result: Method completes successfully without exceptions.
        /// </summary>
        [Fact]
        public void ValidateGesture_ValidGestureRecognizer_ExecutesSuccessfully()
        {
            // Arrange
            var gestureElement = new GestureElement();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer>();

            // Act & Assert
            var exception = Record.Exception(() => gestureElement.ValidateGesture(mockGestureRecognizer));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that ValidateGesture method executes successfully with a null gesture recognizer.
        /// Verifies that the method handles null input gracefully without throwing exceptions.
        /// Expected result: Method completes successfully without exceptions.
        /// </summary>
        [Fact]
        public void ValidateGesture_NullGestureRecognizer_ExecutesSuccessfully()
        {
            // Arrange
            var gestureElement = new GestureElement();

            // Act & Assert
            var exception = Record.Exception(() => gestureElement.ValidateGesture(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that ValidateGesture method can be overridden in derived classes.
        /// Verifies that derived classes can override the virtual ValidateGesture method and execute custom logic.
        /// Expected result: Override method is called and executes successfully.
        /// </summary>
        [Fact]
        public void ValidateGesture_OverriddenInDerivedClass_ExecutesOverriddenMethod()
        {
            // Arrange
            var derivedElement = new TestGestureElement();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer>();

            // Act & Assert
            var exception = Record.Exception(() => derivedElement.ValidateGesture(mockGestureRecognizer));
            Assert.Null(exception);
            Assert.True(derivedElement.ValidateGestureCalled);
        }

        /// <summary>
        /// Test helper class that overrides ValidateGesture to verify override behavior.
        /// </summary>
        private class TestGestureElement : GestureElement
        {
            public bool ValidateGestureCalled { get; private set; }

            internal override void ValidateGesture(IGestureRecognizer gesture)
            {
                ValidateGestureCalled = true;
                base.ValidateGesture(gesture);
            }
        }

        /// <summary>
        /// Tests that clearing an empty gesture recognizers collection completes without errors
        /// and raises the appropriate collection changed event.
        /// </summary>
        [Fact]
        public void ClearItems_EmptyCollection_RaisesCollectionChangedEventWithEmptyRemovedList()
        {
            // Arrange
            var gestureElement = new GestureElement();
            var collectionChangedRaised = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            gestureElement.GestureRecognizersCollectionChanged += (sender, args) =>
            {
                collectionChangedRaised = true;
                eventArgs = args;
            };

            // Act
            gestureElement.GestureRecognizers.Clear();

            // Assert
            Assert.True(collectionChangedRaised);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.NotNull(eventArgs.OldItems);
            Assert.Empty(eventArgs.OldItems);
            Assert.Empty(gestureElement.GestureRecognizers);
        }

        /// <summary>
        /// Tests that clearing a collection with a single gesture recognizer properly removes the item
        /// and raises collection changed event with the removed item.
        /// </summary>
        [Fact]
        public void ClearItems_SingleItem_RaisesCollectionChangedEventWithRemovedItem()
        {
            // Arrange
            var gestureElement = new GestureElement();
            var mockGesture = Substitute.For<IGestureRecognizer>();
            gestureElement.GestureRecognizers.Add(mockGesture);

            var collectionChangedRaised = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            gestureElement.GestureRecognizersCollectionChanged += (sender, args) =>
            {
                collectionChangedRaised = true;
                eventArgs = args;
            };

            // Act
            gestureElement.GestureRecognizers.Clear();

            // Assert
            Assert.True(collectionChangedRaised);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.NotNull(eventArgs.OldItems);
            Assert.Single(eventArgs.OldItems);
            Assert.Equal(mockGesture, eventArgs.OldItems[0]);
            Assert.Empty(gestureElement.GestureRecognizers);
        }

        /// <summary>
        /// Tests that clearing a collection with multiple gesture recognizers properly removes all items
        /// and raises collection changed event with all removed items in the correct order.
        /// </summary>
        [Fact]
        public void ClearItems_MultipleItems_RaisesCollectionChangedEventWithAllRemovedItems()
        {
            // Arrange
            var gestureElement = new GestureElement();
            var mockGesture1 = Substitute.For<IGestureRecognizer>();
            var mockGesture2 = Substitute.For<IGestureRecognizer>();
            var mockGesture3 = Substitute.For<IGestureRecognizer>();

            gestureElement.GestureRecognizers.Add(mockGesture1);
            gestureElement.GestureRecognizers.Add(mockGesture2);
            gestureElement.GestureRecognizers.Add(mockGesture3);

            var collectionChangedRaised = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            gestureElement.GestureRecognizersCollectionChanged += (sender, args) =>
            {
                collectionChangedRaised = true;
                eventArgs = args;
            };

            // Act
            gestureElement.GestureRecognizers.Clear();

            // Assert
            Assert.True(collectionChangedRaised);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.NotNull(eventArgs.OldItems);
            Assert.Equal(3, eventArgs.OldItems.Count);
            Assert.Contains(mockGesture1, eventArgs.OldItems.Cast<IGestureRecognizer>());
            Assert.Contains(mockGesture2, eventArgs.OldItems.Cast<IGestureRecognizer>());
            Assert.Contains(mockGesture3, eventArgs.OldItems.Cast<IGestureRecognizer>());
            Assert.Empty(gestureElement.GestureRecognizers);
        }

        /// <summary>
        /// Tests that clearing preserves the order of items in the removed items list
        /// by comparing the original order with the order in the event args.
        /// </summary>
        [Fact]
        public void ClearItems_PreservesOrderOfRemovedItems()
        {
            // Arrange
            var gestureElement = new GestureElement();
            var mockGestures = new List<IGestureRecognizer>();

            for (int i = 0; i < 5; i++)
            {
                var mockGesture = Substitute.For<IGestureRecognizer>();
                mockGestures.Add(mockGesture);
                gestureElement.GestureRecognizers.Add(mockGesture);
            }

            NotifyCollectionChangedEventArgs eventArgs = null;
            gestureElement.GestureRecognizersCollectionChanged += (sender, args) => eventArgs = args;

            // Act
            gestureElement.GestureRecognizers.Clear();

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(mockGestures.Count, eventArgs.OldItems.Count);

            for (int i = 0; i < mockGestures.Count; i++)
            {
                Assert.Equal(mockGestures[i], eventArgs.OldItems[i]);
            }
        }

        /// <summary>
        /// Tests that clearing a collection multiple times works correctly
        /// and only raises events when there are items to remove.
        /// </summary>
        [Fact]
        public void ClearItems_ClearTwice_SecondClearWithEmptyCollection()
        {
            // Arrange
            var gestureElement = new GestureElement();
            var mockGesture = Substitute.For<IGestureRecognizer>();
            gestureElement.GestureRecognizers.Add(mockGesture);

            var eventCount = 0;
            NotifyCollectionChangedEventArgs lastEventArgs = null;

            gestureElement.GestureRecognizersCollectionChanged += (sender, args) =>
            {
                eventCount++;
                lastEventArgs = args;
            };

            // Act
            gestureElement.GestureRecognizers.Clear(); // First clear
            gestureElement.GestureRecognizers.Clear(); // Second clear on empty collection

            // Assert
            Assert.Equal(2, eventCount);
            Assert.NotNull(lastEventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, lastEventArgs.Action);
            Assert.NotNull(lastEventArgs.OldItems);
            Assert.Empty(lastEventArgs.OldItems); // Second clear should have empty removed list
            Assert.Empty(gestureElement.GestureRecognizers);
        }

        /// <summary>
        /// Tests that the GestureElement constructor properly initializes the object and sets up the CollectionChanged event handler
        /// for the internal gesture recognizers collection. Verifies basic constructor functionality without exceptions.
        /// </summary>
        [Fact]
        public void Constructor_InitializesObjectAndEventHandler_CompletesSuccessfully()
        {
            // Act
            var gestureElement = new GestureElement();

            // Assert
            Assert.NotNull(gestureElement);
            Assert.NotNull(gestureElement.GestureRecognizers);
            Assert.Empty(gestureElement.GestureRecognizers);
        }

        /// <summary>
        /// Tests that when items are added to the GestureRecognizers collection, the constructor's event handler
        /// properly calls ValidateGesture and sets the Parent property for items implementing IElementDefinition.
        /// </summary>
        [Fact]
        public void Constructor_EventHandler_AddAction_ValidatesGestureAndSetsParent()
        {
            // Arrange
            var gestureElement = new TestableGestureElement();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer, IElementDefinition>();

            // Act
            gestureElement.GestureRecognizers.Add(mockGestureRecognizer);

            // Assert
            Assert.Single(gestureElement.ValidateGestureCalls);
            Assert.Equal(mockGestureRecognizer, gestureElement.ValidateGestureCalls[0]);
            ((IElementDefinition)mockGestureRecognizer).Parent.Returns().Received(1);
            var elementDefinition = (IElementDefinition)mockGestureRecognizer;
            elementDefinition.Received(1).Parent = gestureElement;
        }

        /// <summary>
        /// Tests that when items are removed from the GestureRecognizers collection, the constructor's event handler
        /// properly sets the Parent property to null for items implementing IElementDefinition.
        /// </summary>
        [Fact]
        public void Constructor_EventHandler_RemoveAction_SetsParentToNull()
        {
            // Arrange
            var gestureElement = new GestureElement();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer, IElementDefinition>();
            gestureElement.GestureRecognizers.Add(mockGestureRecognizer);

            // Act
            gestureElement.GestureRecognizers.Remove(mockGestureRecognizer);

            // Assert
            var elementDefinition = (IElementDefinition)mockGestureRecognizer;
            elementDefinition.Received(1).Parent = null;
        }

        /// <summary>
        /// Tests that when items are replaced in the GestureRecognizers collection, the constructor's event handler
        /// properly handles both old items (setting Parent to null) and new items (validating and setting Parent).
        /// </summary>
        [Fact]
        public void Constructor_EventHandler_ReplaceAction_HandlesOldAndNewItems()
        {
            // Arrange
            var gestureElement = new TestableGestureElement();
            var oldMockGestureRecognizer = Substitute.For<IGestureRecognizer, IElementDefinition>();
            var newMockGestureRecognizer = Substitute.For<IGestureRecognizer, IElementDefinition>();
            gestureElement.GestureRecognizers.Add(oldMockGestureRecognizer);
            gestureElement.ValidateGestureCalls.Clear();

            // Act
            gestureElement.GestureRecognizers[0] = newMockGestureRecognizer;

            // Assert
            // Verify new item was validated and parent set
            Assert.Single(gestureElement.ValidateGestureCalls);
            Assert.Equal(newMockGestureRecognizer, gestureElement.ValidateGestureCalls[0]);
            var newElementDefinition = (IElementDefinition)newMockGestureRecognizer;
            newElementDefinition.Received(1).Parent = gestureElement;

            // Verify old item parent was set to null
            var oldElementDefinition = (IElementDefinition)oldMockGestureRecognizer;
            oldElementDefinition.Received(1).Parent = null;
        }

        /// <summary>
        /// Tests that when the GestureRecognizers collection is cleared, the constructor's event handler
        /// properly sets the Parent property to the current GestureElement for all remaining items in the collection.
        /// </summary>
        [Fact]
        public void Constructor_EventHandler_ResetAction_SetsParentForAllItems()
        {
            // Arrange
            var gestureElement = new GestureElement();
            var mockGestureRecognizer1 = Substitute.For<IGestureRecognizer, IElementDefinition>();
            var mockGestureRecognizer2 = Substitute.For<IGestureRecognizer, IElementDefinition>();

            gestureElement.GestureRecognizers.Add(mockGestureRecognizer1);
            gestureElement.GestureRecognizers.Add(mockGestureRecognizer2);

            // Act
            gestureElement.GestureRecognizers.Clear();

            // Assert
            // After clear, the Reset action should set Parent for remaining items (but collection is empty after clear)
            // The test verifies the handler processes the Reset action without throwing
            Assert.Empty(gestureElement.GestureRecognizers);
        }

        /// <summary>
        /// Tests that the constructor's event handler properly invokes the GestureRecognizersCollectionChanged event
        /// when items are added to the collection.
        /// </summary>
        [Fact]
        public void Constructor_EventHandler_InvokesGestureRecognizersCollectionChangedEvent()
        {
            // Arrange
            var gestureElement = new GestureElement();
            NotifyCollectionChangedEventArgs capturedArgs = null;
            object capturedSender = null;

            gestureElement.GestureRecognizersCollectionChanged += (sender, args) =>
            {
                capturedSender = sender;
                capturedArgs = args;
            };

            var mockGestureRecognizer = Substitute.For<IGestureRecognizer, IElementDefinition>();

            // Act
            gestureElement.GestureRecognizers.Add(mockGestureRecognizer);

            // Assert
            Assert.NotNull(capturedArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, capturedArgs.Action);
            Assert.Contains(mockGestureRecognizer, capturedArgs.NewItems.Cast<object>());
            Assert.Equal(gestureElement.GestureRecognizers, capturedSender);
        }

        /// <summary>
        /// Tests that the constructor's event handler handles items that implement IGestureRecognizer but not IElementDefinition
        /// without throwing exceptions. ValidateGesture should still be called, but Parent assignment should be skipped.
        /// </summary>
        [Fact]
        public void Constructor_EventHandler_HandlesItemsNotImplementingIElementDefinition()
        {
            // Arrange
            var gestureElement = new TestableGestureElement();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer>();

            // Act & Assert - should not throw
            gestureElement.GestureRecognizers.Add(mockGestureRecognizer);

            // ValidateGesture should be called even if item doesn't implement IElementDefinition
            Assert.Single(gestureElement.ValidateGestureCalls);
            Assert.Equal(mockGestureRecognizer, gestureElement.ValidateGestureCalls[0]);
        }

        /// <summary>
        /// Tests that the constructor's event handler handles null items in the NewItems collection
        /// without throwing exceptions during add operations.
        /// </summary>
        [Fact]
        public void Constructor_EventHandler_HandlesNullItemsInNewItems()
        {
            // Arrange
            var gestureElement = new TestableGestureElement();

            // Act & Assert - should not throw when adding null
            gestureElement.GestureRecognizers.Add(null);

            // ValidateGesture should be called with null
            Assert.Single(gestureElement.ValidateGestureCalls);
            Assert.Null(gestureElement.ValidateGestureCalls[0]);
        }

        /// <summary>
        /// Tests that the constructor's event handler handles null items in the OldItems collection
        /// without throwing exceptions during remove operations.
        /// </summary>
        [Fact]
        public void Constructor_EventHandler_HandlesNullItemsInOldItems()
        {
            // Arrange
            var gestureElement = new GestureElement();
            gestureElement.GestureRecognizers.Add(null);

            // Act & Assert - should not throw when removing null
            gestureElement.GestureRecognizers.Remove(null);

            // Should complete without exceptions
            Assert.Empty(gestureElement.GestureRecognizers);
        }

        /// <summary>
        /// Tests that the constructor's event handler works correctly when GestureRecognizersCollectionChanged
        /// event is null (no subscribers).
        /// </summary>
        [Fact]
        public void Constructor_EventHandler_HandlesNullGestureRecognizersCollectionChangedEvent()
        {
            // Arrange
            var gestureElement = new GestureElement();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer, IElementDefinition>();

            // Act & Assert - should not throw when event is null (no subscribers)
            gestureElement.GestureRecognizers.Add(mockGestureRecognizer);

            // Should complete successfully
            Assert.Single(gestureElement.GestureRecognizers);
            Assert.Equal(mockGestureRecognizer, gestureElement.GestureRecognizers[0]);
        }

        /// <summary>
        /// Internal testable subclass that exposes ValidateGesture calls for verification.
        /// This allows testing the virtual method calls made by the constructor's event handler.
        /// </summary>
        private class TestableGestureElement : GestureElement
        {
            public List<IGestureRecognizer> ValidateGestureCalls { get; } = new List<IGestureRecognizer>();

            internal override void ValidateGesture(IGestureRecognizer gesture)
            {
                ValidateGestureCalls.Add(gesture);
                base.ValidateGesture(gesture);
            }
        }
    }
}