#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ObservableWrapperTests : BaseTestFixture
    {
        [Fact]
        public void Constructor()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            Assert.Empty(wrapper);

            Assert.Throws<ArgumentNullException>(() => new ObservableWrapper<View, View>(null));
        }

        [Fact]
        public void IgnoresInternallyAdded()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            var child = new View();

            observableCollection.Add(child);

            Assert.Empty(wrapper);
        }

        [Fact]
        public void TracksExternallyAdded()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            var child = new Button();

            wrapper.Add(child);

            Assert.Equal(child, wrapper[0]);
            Assert.Equal(child, observableCollection[0]);
        }

        [Fact]
        public void AddWithInternalItemsAlreadyAdded()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            var view = new View();
            observableCollection.Add(view);

            var btn = new Button();

            wrapper.Add(btn);

            Assert.Equal(btn, wrapper[0]);
            Assert.Single(wrapper);

            Assert.Contains(btn, observableCollection);
            Assert.Contains(view, observableCollection);
            Assert.Equal(2, observableCollection.Count);
        }

        [Fact]
        public void IgnoresInternallyAddedSameType()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            var child = new View();

            observableCollection.Add(child);

            Assert.Empty(wrapper);
        }

        [Fact]
        public void TracksExternallyAddedSameType()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            var child = new Button();

            wrapper.Add(child);

            Assert.Equal(child, wrapper[0]);
            Assert.Equal(child, observableCollection[0]);
        }

        [Fact]
        public void AddWithInternalItemsAlreadyAddedSameType()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            var view = new View();
            observableCollection.Add(view);

            var btn = new Button();

            wrapper.Add(btn);

            Assert.Equal(btn, wrapper[0]);
            Assert.Single(wrapper);

            Assert.Contains(btn, observableCollection);
            Assert.Contains(view, observableCollection);
            Assert.Equal(2, observableCollection.Count);
        }

        [Fact]
        public void CannotRemoveInternalItem()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            var child = new View();

            observableCollection.Add(child);

            Assert.Empty(wrapper);

            Assert.False(wrapper.Remove(child));

            Assert.Contains(child, observableCollection);
        }

        [Fact]
        public void ReadOnly()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            Assert.False(wrapper.IsReadOnly);

            wrapper.Add(new Button());

            wrapper.IsReadOnly = true;

            Assert.True(wrapper.IsReadOnly);

            Assert.Throws<NotSupportedException>(() => wrapper.Remove(wrapper[0]));
            Assert.Throws<NotSupportedException>(() => wrapper.Add(new Button()));
            Assert.Throws<NotSupportedException>(() => wrapper.RemoveAt(0));
            Assert.Throws<NotSupportedException>(() => wrapper.Insert(0, new Button()));
            Assert.Throws<NotSupportedException>(wrapper.Clear);
        }

        [Fact]
        public void Indexer()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            wrapper.Add(new Button());

            var newButton = new Button();

            wrapper[0] = newButton;

            Assert.Equal(newButton, wrapper[0]);
        }

        [Fact]
        public void IndexerSameType()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            wrapper.Add(new Button());

            var newButton = new Button();

            wrapper[0] = newButton;

            Assert.Equal(newButton, wrapper[0]);
        }

        [Fact]
        public void CopyTo()
        {
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            var child1 = new Button();
            var child2 = new Button();
            var child3 = new Button();
            var child4 = new Button();
            var child5 = new Button();

            observableCollection.Add(new Stepper());
            wrapper.Add(child1);
            observableCollection.Add(new Button());
            wrapper.Add(child2);
            wrapper.Add(child3);
            wrapper.Add(child4);
            wrapper.Add(child5);
            observableCollection.Add(new Button());

            var target = new View[30];
            wrapper.CopyTo(target, 2);

            Assert.Equal(target[2], child1);
            Assert.Equal(target[3], child2);
            Assert.Equal(target[4], child3);
            Assert.Equal(target[5], child4);
            Assert.Equal(target[6], child5);
        }

        [Fact]
        public void INCCSimpleAdd()
        {
            var oc = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(oc);

            var child = new Button();

            Button addedResult = null;
            int addIndex = -1;
            wrapper.CollectionChanged += (sender, args) =>
            {
                addedResult = args.NewItems[0] as Button;
                addIndex = args.NewStartingIndex;
            };

            wrapper.Add(child);

            Assert.Equal(0, addIndex);
            Assert.Equal(child, addedResult);
        }

        [Fact]
        public void INCCSimpleAddToInner()
        {
            var oc = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(oc);

            var child = new Button();

            Button addedResult = null;
            int addIndex = -1;
            wrapper.CollectionChanged += (sender, args) =>
            {
                addedResult = args.NewItems[0] as Button;
                addIndex = args.NewStartingIndex;
            };

            oc.Add(child);

            Assert.Equal(-1, addIndex);
            Assert.Null(addedResult);
        }

        [Fact]
        public void INCCComplexAdd()
        {
            var oc = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(oc);

            oc.Add(new Stepper());

            var child = new Button();

            Button addedResult = null;
            int addIndex = -1;
            wrapper.CollectionChanged += (sender, args) =>
            {
                addedResult = args.NewItems[0] as Button;
                addIndex = args.NewStartingIndex;
            };

            wrapper.Add(child);

            Assert.Equal(0, addIndex);
            Assert.Equal(child, addedResult);
        }

        [Fact]
        public void INCCSimpleRemove()
        {
            var oc = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(oc);

            var child = new Button();
            wrapper.Add(child);

            Button removedResult = null;
            int removeIndex = -1;
            wrapper.CollectionChanged += (sender, args) =>
            {
                removedResult = args.OldItems[0] as Button;
                removeIndex = args.OldStartingIndex;
            };

            wrapper.Remove(child);

            Assert.Equal(0, removeIndex);
            Assert.Equal(child, removedResult);
        }

        [Fact]
        public void INCCSimpleRemoveFromInner()
        {
            var oc = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(oc);

            var child = new Button();
            oc.Add(child);

            Button addedResult = null;
            int addIndex = -1;
            wrapper.CollectionChanged += (sender, args) =>
            {
                addedResult = args.OldItems[0] as Button;
                addIndex = args.OldStartingIndex;
            };

            oc.Remove(child);

            Assert.Equal(-1, addIndex);
            Assert.Null(addedResult);
        }

        [Fact]
        public void INCCComplexRemove()
        {
            var oc = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(oc);

            oc.Add(new Stepper());

            var child = new Button();
            wrapper.Add(child);

            Button removedResult = null;
            int removeIndex = -1;
            wrapper.CollectionChanged += (sender, args) =>
            {
                removedResult = args.OldItems[0] as Button;
                removeIndex = args.OldStartingIndex;
            };

            wrapper.Remove(child);

            Assert.Equal(child, removedResult);
            Assert.Equal(0, removeIndex);
        }

        [Fact]
        public void INCCComplexRemoveLast()
        {
            var oc = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(oc);

            oc.Add(new Stepper());

            wrapper.Add(new Button());
            wrapper.Add(new Button());
            var child = new Button();
            wrapper.Add(child);

            Button removedResult = null;
            int removeIndex = -1;
            wrapper.CollectionChanged += (sender, args) =>
            {
                removedResult = args.OldItems[0] as Button;
                removeIndex = args.OldStartingIndex;
            };

            wrapper.Remove(child);

            Assert.Equal(child, removedResult);
            Assert.Equal(2, removeIndex);
        }

        [Fact]
        public void INCCReplace()
        {
            var oc = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(oc);

            var child1 = new Button();
            var child2 = new Button();

            wrapper.Add(child1);

            int index = -1;
            Button oldItem = null;
            Button newItem = null;
            wrapper.CollectionChanged += (sender, args) =>
            {
                index = args.NewStartingIndex;
                oldItem = args.OldItems[0] as Button;
                newItem = args.NewItems[0] as Button;
            };

            wrapper[0] = child2;

            Assert.Equal(0, index);
            Assert.Equal(child1, oldItem);
            Assert.Equal(child2, newItem);
        }

        [Fact]
        public void Clear()
        {
            var oc = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(oc);

            oc.Add(new Stepper());

            wrapper.Add(new Button());
            wrapper.Add(new Button());

            wrapper.Clear();
            Assert.Single(oc);
            Assert.Empty(wrapper);
        }

        [Fact]
        public void DifferentTypes()
        {
            var oc = new ObservableCollection<Element>();
            var wrapper = new ObservableWrapper<Element, Button>(oc);

            // Wrong type!
            oc.Add(new Label());

            var child1 = new Button();
            var child2 = new Button();
            wrapper.Add(child1);
            wrapper.Add(child2);

            // Do things that might cast
            foreach (var item in wrapper)
            { }
            var target = new Button[4];
            wrapper.CopyTo(target, 2);
            Assert.Equal(target[2], child1);
            Assert.Equal(target[3], child2);
        }

        [Fact]
        public void CopyToArrayBaseType()
        {
            var oc = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(oc);

            oc.Add(new Stepper());

            var child1 = new Button();
            var child2 = new Button();
            wrapper.Add(child1);
            wrapper.Add(child2);

            var target = new View[4];
            wrapper.CopyTo((Array)target, 2);
            Assert.Equal(target[2], child1);
            Assert.Equal(target[3], child2);
        }

        /// <summary>
        /// Tests that the Remove method throws ArgumentNullException when a null item is passed.
        /// This test covers the null parameter validation in the Remove method.
        /// </summary>
        [Fact]
        public void Remove_NullItem_ThrowsArgumentNullException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => wrapper.Remove(null));
        }

        /// <summary>
        /// Tests that Add(object) throws ArgumentNullException when passed null value.
        /// </summary>
        [Fact]
        public void Add_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((IList)wrapper).Add(null));
        }

        /// <summary>
        /// Tests that Add(object) throws InvalidCastException when passed object of incorrect type.
        /// </summary>
        [Fact]
        public void Add_InvalidCastType_ThrowsInvalidCastException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var invalidObject = "not a button";

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => ((IList)wrapper).Add(invalidObject));
        }

        /// <summary>
        /// Tests that Add(object) throws NotSupportedException when collection is read-only.
        /// </summary>
        [Fact]
        public void Add_ReadOnlyCollection_ThrowsNotSupportedException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            wrapper.IsReadOnly = true;
            var button = new Button();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => ((IList)wrapper).Add(button));
        }

        /// <summary>
        /// Tests that Add(object) successfully adds valid object and returns correct index.
        /// </summary>
        [Fact]
        public void Add_ValidObject_AddsSuccessfullyAndReturnsIndex()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button = new Button();

            // Act
            int result = ((IList)wrapper).Add(button);

            // Assert
            Assert.Equal(0, result);
            Assert.Equal(button, wrapper[0]);
            Assert.Contains(button, observableCollection);
        }

        /// <summary>
        /// Tests that Add(object) handles duplicate items correctly by not adding and returning existing index.
        /// </summary>
        [Fact]
        public void Add_DuplicateObject_ReturnsExistingIndex()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button = new Button();
            ((IList)wrapper).Add(button);

            // Act
            int result = ((IList)wrapper).Add(button);

            // Assert
            Assert.Equal(0, result);
            Assert.Single(wrapper);
            Assert.Equal(button, wrapper[0]);
        }

        /// <summary>
        /// Tests that Add(object) returns correct index when adding to non-empty collection.
        /// </summary>
        [Fact]
        public void Add_ToNonEmptyCollection_ReturnsCorrectIndex()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var firstButton = new Button();
            var secondButton = new Button();
            ((IList)wrapper).Add(firstButton);

            // Act
            int result = ((IList)wrapper).Add(secondButton);

            // Assert
            Assert.Equal(1, result);
            Assert.Equal(2, wrapper.Count);
            Assert.Equal(secondButton, wrapper[1]);
        }

        /// <summary>
        /// Tests that Remove(object) successfully removes a valid TRestrict item that is owned.
        /// Input: A Button object that has been added to the wrapper (owned).
        /// Expected: Item is removed and method returns via the underlying Remove(TRestrict) method.
        /// </summary>
        [Fact]
        public void Remove_ValidOwnedItem_RemovesSuccessfully()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button = new Button();
            wrapper.Add(button);

            // Act
            ((IList)wrapper).Remove((object)button);

            // Assert
            Assert.DoesNotContain(button, wrapper);
            Assert.False(button.Owned);
        }

        /// <summary>
        /// Tests that Remove(object) handles null input by throwing ArgumentNullException.
        /// Input: null object value.
        /// Expected: ArgumentNullException is thrown from the underlying Remove(TRestrict) method.
        /// </summary>
        [Fact]
        public void Remove_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((IList)wrapper).Remove(null));
        }

        /// <summary>
        /// Tests that Remove(object) throws InvalidCastException when given an object that cannot be cast to TRestrict.
        /// Input: A View object (which cannot be cast to Button).
        /// Expected: InvalidCastException is thrown during the cast operation.
        /// </summary>
        [Fact]
        public void Remove_InvalidCastObject_ThrowsInvalidCastException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var view = new View(); // Cannot be cast to Button

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => ((IList)wrapper).Remove((object)view));
        }

        /// <summary>
        /// Tests that Remove(object) throws NotSupportedException when the collection is read-only.
        /// Input: Valid Button object when IsReadOnly is true.
        /// Expected: NotSupportedException is thrown from the underlying Remove(TRestrict) method.
        /// </summary>
        [Fact]
        public void Remove_ReadOnlyCollection_ThrowsNotSupportedException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button = new Button();
            wrapper.Add(button);
            wrapper.IsReadOnly = true;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => ((IList)wrapper).Remove((object)button));
        }

        /// <summary>
        /// Tests that Remove(object) returns without throwing when attempting to remove an unowned item.
        /// Input: A Button object that was added directly to the underlying collection (not owned).
        /// Expected: No exception is thrown, method completes successfully without removing the item.
        /// </summary>
        [Fact]
        public void Remove_UnownedItem_CompletesWithoutThrowing()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button = new Button();
            observableCollection.Add(button); // Add directly to underlying collection, so not owned

            // Act & Assert (should not throw)
            ((IList)wrapper).Remove((object)button);

            // Verify item is still in the underlying collection
            Assert.Contains(button, observableCollection);
        }

        /// <summary>
        /// Tests that Remove(object) throws InvalidCastException for completely unrelated object types.
        /// Input: A string object (which cannot be cast to Button).
        /// Expected: InvalidCastException is thrown during the cast operation.
        /// </summary>
        [Fact]
        public void Remove_UnrelatedObjectType_ThrowsInvalidCastException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => ((IList)wrapper).Remove("not a button"));
        }

        /// <summary>
        /// Tests that the IsFixedSize property correctly delegates to the underlying ObservableCollection's IsFixedSize property.
        /// Verifies that the property returns false for ObservableCollection which is a dynamic, resizable collection.
        /// </summary>
        [Fact]
        public void IsFixedSize_WithObservableCollection_ReturnsFalse()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act
            bool isFixedSize = wrapper.IsFixedSize;

            // Assert
            Assert.False(isFixedSize);
        }

        /// <summary>
        /// Tests that IsSynchronized property returns the same value as the underlying ObservableCollection's IsSynchronized property.
        /// Verifies proper delegation to the wrapped collection's synchronization status.
        /// Expected result: IsSynchronized should match the underlying collection's IsSynchronized value.
        /// </summary>
        [Fact]
        public void IsSynchronized_ReturnsUnderlyingCollectionIsSynchronized_MatchesExpectedValue()
        {
            // Arrange
            var innerCollection = new ObservableCollection<Element>();
            var wrapper = new ObservableWrapper<Element, Element>(innerCollection);
            var expectedIsSynchronized = ((IList)innerCollection).IsSynchronized;

            // Act
            var actualIsSynchronized = wrapper.IsSynchronized;

            // Assert
            Assert.Equal(expectedIsSynchronized, actualIsSynchronized);
        }

        /// <summary>
        /// Tests that the SyncRoot property returns a non-null object.
        /// Validates that the property delegation to the underlying list works correctly.
        /// </summary>
        [Fact]
        public void SyncRoot_ReturnsNonNull_Success()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act
            object syncRoot = wrapper.SyncRoot;

            // Assert
            Assert.NotNull(syncRoot);
        }

        /// <summary>
        /// Tests that the SyncRoot property returns the same object on multiple calls.
        /// Validates consistency of the SyncRoot property across multiple accesses.
        /// </summary>
        [Fact]
        public void SyncRoot_ReturnsSameObjectOnMultipleCalls_Success()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act
            object syncRoot1 = wrapper.SyncRoot;
            object syncRoot2 = wrapper.SyncRoot;

            // Assert
            Assert.Same(syncRoot1, syncRoot2);
        }

        /// <summary>
        /// Tests that the SyncRoot property delegates to the underlying list's SyncRoot.
        /// Validates that the wrapper correctly exposes the underlying collection's synchronization object.
        /// </summary>
        [Fact]
        public void SyncRoot_DelegatesToUnderlyingList_Success()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act
            object wrapperSyncRoot = wrapper.SyncRoot;
            object collectionSyncRoot = ((IList)observableCollection).SyncRoot;

            // Assert
            Assert.Same(collectionSyncRoot, wrapperSyncRoot);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentNullException when a null item is passed.
        /// This test ensures proper null parameter validation.
        /// </summary>
        [Fact]
        public void Add_NullItem_ThrowsArgumentNullException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => wrapper.Add(null));
        }

        /// <summary>
        /// Tests that Add method does not add duplicate items when the same item is added twice.
        /// This test verifies the early return behavior when an item already exists in the collection.
        /// </summary>
        [Fact]
        public void Add_ItemAlreadyExists_DoesNotAddDuplicate()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button = new Button();

            // Act
            wrapper.Add(button);
            wrapper.Add(button); // Try to add the same item again

            // Assert
            Assert.Single(wrapper);
            Assert.Single(observableCollection);
            Assert.Equal(button, wrapper[0]);
        }
    }

    public partial class ObservableWrapperConstructorTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that ObservableWrapper constructor successfully creates an instance when provided with a valid empty ObservableCollection.
        /// Verifies the wrapper is properly initialized and empty.
        /// </summary>
        [Fact]
        public void Constructor_ValidEmptyCollection_CreatesEmptyWrapper()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();

            // Act
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Assert
            Assert.NotNull(wrapper);
            Assert.Empty(wrapper);
            Assert.Equal(0, wrapper.Count);
        }

        /// <summary>
        /// Tests that ObservableWrapper constructor successfully creates an instance when provided with a valid non-empty ObservableCollection.
        /// Verifies the wrapper properly reflects the initial collection state.
        /// </summary>
        [Fact]
        public void Constructor_ValidNonEmptyCollection_CreatesWrapperWithCorrectCount()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var view1 = new View();
            var view2 = new View();
            observableCollection.Add(view1);
            observableCollection.Add(view2);

            // Act
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Assert
            Assert.NotNull(wrapper);
            Assert.Equal(0, wrapper.Count); // Wrapper only tracks TRestrict items, not TTrack items
        }

        /// <summary>
        /// Tests that ObservableWrapper constructor throws ArgumentNullException when provided with a null ObservableCollection parameter.
        /// Verifies the exception has the correct parameter name.
        /// </summary>
        [Fact]
        public void Constructor_NullCollection_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ObservableWrapper<View, Button>(null));
            Assert.Equal("list", exception.ParamName);
        }

        /// <summary>
        /// Tests that ObservableWrapper constructor successfully creates an instance when TTrack and TRestrict are the same type.
        /// Verifies proper initialization with matching generic type parameters.
        /// </summary>
        [Fact]
        public void Constructor_SameGenericTypes_CreatesValidWrapper()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();

            // Act
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            // Assert
            Assert.NotNull(wrapper);
            Assert.Empty(wrapper);
            Assert.Equal(0, wrapper.Count);
        }

        /// <summary>
        /// Tests that ObservableWrapper constructor properly subscribes to the CollectionChanged event of the provided ObservableCollection.
        /// Verifies event subscription by triggering a collection change and observing the response.
        /// </summary>
        [Fact]
        public void Constructor_ValidCollection_SubscribesToCollectionChangedEvent()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            bool eventTriggered = false;

            wrapper.CollectionChanged += (sender, e) => eventTriggered = true;

            // Act
            var button = new Button { Owned = true };
            observableCollection.Add(button);

            // Assert
            Assert.True(eventTriggered);
        }

        /// <summary>
        /// Tests that ObservableWrapper constructor handles ObservableCollection with maximum capacity gracefully.
        /// Verifies proper initialization with a very large collection.
        /// </summary>
        [Fact]
        public void Constructor_LargeCollection_CreatesValidWrapper()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            for (int i = 0; i < 1000; i++)
            {
                observableCollection.Add(new View());
            }

            // Act
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Assert
            Assert.NotNull(wrapper);
            Assert.Equal(0, wrapper.Count); // No Button items in collection
        }

        /// <summary>
        /// Tests that ObservableWrapper constructor maintains proper state when collection contains mixed item types.
        /// Verifies wrapper correctly filters to only TRestrict type items.
        /// </summary>
        [Fact]
        public void Constructor_CollectionWithMixedTypes_FiltersCorrectly()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            observableCollection.Add(new View());
            var button = new Button { Owned = true };
            observableCollection.Add(button);

            // Act
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Assert
            Assert.NotNull(wrapper);
            Assert.Equal(1, wrapper.Count); // Only the Button should be visible in wrapper
        }

        /// <summary>
        /// Tests that ObservableWrapper constructor preserves IsReadOnly property default state.
        /// Verifies the wrapper is initially not read-only.
        /// </summary>
        [Fact]
        public void Constructor_ValidCollection_IsReadOnlyDefaultsFalse()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();

            // Act
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Assert
            Assert.False(wrapper.IsReadOnly);
        }
    }


    public partial class ObservableWrapperCopyToTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that CopyTo throws ArgumentException when destination array does not have enough space
        /// from destIndex to accommodate all items in the wrapper.
        /// </summary>
        [Theory]
        [InlineData(3, 0, 2)] // Array length 3, destIndex 0, Count 2 -> 3-0=3 >= 2, should work
        [InlineData(2, 0, 3)] // Array length 2, destIndex 0, Count 3 -> 2-0=2 < 3, should throw
        [InlineData(5, 3, 3)] // Array length 5, destIndex 3, Count 3 -> 5-3=2 < 3, should throw  
        [InlineData(5, 2, 3)] // Array length 5, destIndex 2, Count 3 -> 5-2=3 >= 3, should work
        [InlineData(1, 0, 1)] // Array length 1, destIndex 0, Count 1 -> 1-0=1 >= 1, should work
        [InlineData(1, 1, 1)] // Array length 1, destIndex 1, Count 1 -> 1-1=0 < 1, should throw
        public void CopyTo_ArraySpaceValidation_ThrowsOrSucceedsAsExpected(int arrayLength, int destIndex, int itemCount)
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            // Add the specified number of items to achieve the desired Count
            for (int i = 0; i < itemCount; i++)
            {
                wrapper.Add(new Button());
            }

            var targetArray = new View[arrayLength];

            // Act & Assert
            if (arrayLength - destIndex < itemCount)
            {
                var exception = Assert.Throws<ArgumentException>(() => wrapper.CopyTo(targetArray, destIndex));
                Assert.Contains("Destination array was not long enough", exception.Message);
            }
            else
            {
                // Should not throw
                wrapper.CopyTo(targetArray, destIndex);

                // Verify items were copied correctly
                for (int i = 0; i < itemCount; i++)
                {
                    Assert.NotNull(targetArray[destIndex + i]);
                }
            }
        }

        /// <summary>
        /// Tests CopyTo with various edge cases for destIndex parameter.
        /// </summary>
        [Theory]
        [InlineData(-1)] // Negative destIndex
        [InlineData(int.MaxValue)] // Very large destIndex
        public void CopyTo_InvalidDestIndex_ThrowsArgumentException(int destIndex)
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);
            wrapper.Add(new Button()); // Add one item

            var targetArray = new View[10];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => wrapper.CopyTo(targetArray, destIndex));
        }

        /// <summary>
        /// Tests CopyTo with null array parameter.
        /// </summary>
        [Fact]
        public void CopyTo_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);
            wrapper.Add(new Button());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => wrapper.CopyTo(null, 0));
        }

        /// <summary>
        /// Tests CopyTo with empty wrapper (Count = 0).
        /// Should succeed regardless of array size and destIndex (within bounds).
        /// </summary>
        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(10, 5)]
        public void CopyTo_EmptyWrapper_Succeeds(int arrayLength, int destIndex)
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);
            // Don't add any items - Count should be 0

            var targetArray = new View[arrayLength];

            // Act & Assert
            if (destIndex <= arrayLength)
            {
                // Should not throw for empty collection
                wrapper.CopyTo(targetArray, destIndex);

                // Array should remain unchanged (all nulls)
                for (int i = 0; i < arrayLength; i++)
                {
                    Assert.Null(targetArray[i]);
                }
            }
        }

        /// <summary>
        /// Tests CopyTo with exact space requirement - boundary condition.
        /// </summary>
        [Fact]
        public void CopyTo_ExactSpaceRequired_Succeeds()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            var button1 = new Button();
            var button2 = new Button();
            wrapper.Add(button1);
            wrapper.Add(button2);
            // Count = 2

            var targetArray = new View[3]; // Array length 3, destIndex 1 -> 3-1=2, exactly enough space

            // Act
            wrapper.CopyTo(targetArray, 1);

            // Assert
            Assert.Null(targetArray[0]); // First element should be null
            Assert.Equal(button1, targetArray[1]);
            Assert.Equal(button2, targetArray[2]);
        }

        /// <summary>
        /// Tests CopyTo behavior when wrapper contains mixed owned and non-owned items.
        /// Only owned items should be copied.
        /// </summary>
        [Fact]
        public void CopyTo_MixedOwnedItems_CopiesOnlyOwnedItems()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            // Add non-owned item directly to collection
            observableCollection.Add(new Stepper()); // This won't be owned

            var ownedButton1 = new Button();
            var ownedButton2 = new Button();
            wrapper.Add(ownedButton1); // These will be owned
            wrapper.Add(ownedButton2);

            // Add another non-owned item
            observableCollection.Add(new Label()); // This won't be owned

            var targetArray = new View[10];

            // Act
            wrapper.CopyTo(targetArray, 2);

            // Assert
            // Only the 2 owned buttons should be copied
            Assert.Null(targetArray[0]);
            Assert.Null(targetArray[1]);
            Assert.Equal(ownedButton1, targetArray[2]);
            Assert.Equal(ownedButton2, targetArray[3]);
            Assert.Null(targetArray[4]); // No more items
        }

        /// <summary>
        /// Tests CopyTo with zero destIndex and full array utilization.
        /// </summary>
        [Fact]
        public void CopyTo_ZeroDestIndexFullArray_Succeeds()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, View>(observableCollection);

            var button1 = new Button();
            var button2 = new Button();
            var button3 = new Button();
            wrapper.Add(button1);
            wrapper.Add(button2);
            wrapper.Add(button3);

            var targetArray = new View[3]; // Exact size needed

            // Act
            wrapper.CopyTo(targetArray, 0);

            // Assert
            Assert.Equal(button1, targetArray[0]);
            Assert.Equal(button2, targetArray[1]);
            Assert.Equal(button3, targetArray[2]);
        }
    }


    public partial class ObservableWrapperInsertTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Insert throws ArgumentNullException when item parameter is null.
        /// Validates the null check behavior and ensures proper exception is thrown.
        /// </summary>
        [Fact]
        public void Insert_NullItem_ThrowsArgumentNullException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => wrapper.Insert(0, null));
            Assert.Equal("item", exception.ParamName);
        }

        /// <summary>
        /// Tests that Insert correctly inserts an item at the beginning of an empty collection.
        /// Validates that the item's Owned property is set and the item is properly positioned.
        /// </summary>
        [Fact]
        public void Insert_ValidItemAtBeginningOfEmptyCollection_InsertsCorrectly()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button = new Button();

            // Act
            wrapper.Insert(0, button);

            // Assert
            Assert.Single(wrapper);
            Assert.Equal(button, wrapper[0]);
            Assert.True(button.Owned);
            Assert.Contains(button, observableCollection);
        }

        /// <summary>
        /// Tests that Insert correctly inserts an item at the end of a collection with existing items.
        /// Validates proper positioning and that existing items remain unaffected.
        /// </summary>
        [Fact]
        public void Insert_ValidItemAtEnd_InsertsCorrectly()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button1 = new Button();
            var button2 = new Button();

            wrapper.Add(button1);

            // Act
            wrapper.Insert(1, button2);

            // Assert
            Assert.Equal(2, wrapper.Count);
            Assert.Equal(button1, wrapper[0]);
            Assert.Equal(button2, wrapper[1]);
            Assert.True(button2.Owned);
        }

        /// <summary>
        /// Tests that Insert correctly inserts an item in the middle of a collection.
        /// Validates that existing items are shifted properly and the new item is positioned correctly.
        /// </summary>
        [Fact]
        public void Insert_ValidItemInMiddle_InsertsCorrectly()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button1 = new Button();
            var button2 = new Button();
            var button3 = new Button();

            wrapper.Add(button1);
            wrapper.Add(button3);

            // Act
            wrapper.Insert(1, button2);

            // Assert
            Assert.Equal(3, wrapper.Count);
            Assert.Equal(button1, wrapper[0]);
            Assert.Equal(button2, wrapper[1]);
            Assert.Equal(button3, wrapper[2]);
            Assert.True(button2.Owned);
        }

        /// <summary>
        /// Tests Insert behavior with mixed internal and external items in the underlying collection.
        /// Validates that the index calculation works correctly when internal items are present.
        /// </summary>
        [Fact]
        public void Insert_WithInternalItemsPresent_InsertsAtCorrectPosition()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Add an internal item directly to the collection
            var internalView = new View();
            observableCollection.Add(internalView);

            var button1 = new Button();
            var button2 = new Button();

            wrapper.Add(button1);

            // Act
            wrapper.Insert(1, button2);

            // Assert
            Assert.Equal(2, wrapper.Count);
            Assert.Equal(button1, wrapper[0]);
            Assert.Equal(button2, wrapper[1]);
            Assert.True(button2.Owned);

            // Verify the underlying collection has both the internal item and the buttons
            Assert.Equal(3, observableCollection.Count);
            Assert.Contains(internalView, observableCollection);
            Assert.Contains(button1, observableCollection);
            Assert.Contains(button2, observableCollection);
        }
    }


    public partial class ObservableWrapperInsertObjectTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Insert with a valid Button object successfully inserts the item into the collection.
        /// Input: valid Button object and valid index.
        /// Expected: Button is inserted at the specified position.
        /// </summary>
        [Fact]
        public void Insert_ValidButtonObject_InsertsSuccessfully()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button = new Button();
            object buttonObject = button;

            // Act
            wrapper.Insert(0, buttonObject);

            // Assert
            Assert.Single(wrapper);
            Assert.Equal(button, wrapper[0]);
            Assert.True(button.Owned);
        }

        /// <summary>
        /// Tests that Insert with null value successfully inserts null into the collection.
        /// Input: null value and valid index.
        /// Expected: ArgumentNullException is thrown by the strongly-typed Insert method.
        /// </summary>
        [Fact]
        public void Insert_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => wrapper.Insert(0, null));
        }

        /// <summary>
        /// Tests that Insert with an object that cannot be cast to TRestrict throws InvalidCastException.
        /// Input: string object (cannot be cast to Button) and valid index.
        /// Expected: InvalidCastException is thrown during the cast operation.
        /// </summary>
        [Fact]
        public void Insert_InvalidCastObject_ThrowsInvalidCastException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            object invalidObject = "not a button";

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => wrapper.Insert(0, invalidObject));
        }

        /// <summary>
        /// Tests that Insert throws NotSupportedException when the collection is read-only.
        /// Input: valid Button object and valid index, but IsReadOnly is true.
        /// Expected: NotSupportedException is thrown.
        /// </summary>
        [Fact]
        public void Insert_ReadOnlyCollection_ThrowsNotSupportedException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            wrapper.IsReadOnly = true;
            var button = new Button();
            object buttonObject = button;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => wrapper.Insert(0, buttonObject));
        }

        /// <summary>
        /// Tests that Insert works correctly with different valid index positions.
        /// Input: valid Button objects and boundary index values.
        /// Expected: Items are inserted at the correct positions.
        /// </summary>
        [Theory]
        [InlineData(0)] // Insert at beginning
        [InlineData(1)] // Insert at end when collection has one item
        public void Insert_ValidIndexBoundaries_InsertsAtCorrectPosition(int index)
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);

            if (index > 0)
            {
                // Add an initial item if we're testing insertion at position > 0
                wrapper.Add(new Button());
            }

            var newButton = new Button();
            object buttonObject = newButton;

            // Act
            wrapper.Insert(index, buttonObject);

            // Assert
            Assert.Equal(newButton, wrapper[index]);
            Assert.True(newButton.Owned);
        }

        /// <summary>
        /// Tests that Insert with a View object (base type) that can be cast to Button works correctly.
        /// Input: View object that is actually a Button instance and valid index.
        /// Expected: Item is successfully inserted and cast to Button.
        /// </summary>
        [Fact]
        public void Insert_ViewObjectThatIsButton_InsertsSuccessfully()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var button = new Button();
            object viewObject = (View)button; // Cast Button to View, then to object

            // Act
            wrapper.Insert(0, viewObject);

            // Assert
            Assert.Single(wrapper);
            Assert.Equal(button, wrapper[0]);
            Assert.True(button.Owned);
        }

        /// <summary>
        /// Tests that Insert with a View object that cannot be cast to Button throws InvalidCastException.
        /// Input: View object (not Button) and valid index.
        /// Expected: InvalidCastException is thrown during the cast operation.
        /// </summary>
        [Fact]
        public void Insert_ViewObjectThatIsNotButton_ThrowsInvalidCastException()
        {
            // Arrange
            var observableCollection = new ObservableCollection<View>();
            var wrapper = new ObservableWrapper<View, Button>(observableCollection);
            var view = new View(); // Create a View that is not a Button
            object viewObject = view;

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => wrapper.Insert(0, viewObject));
        }
    }
}