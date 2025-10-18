#nullable disable

#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class AttachedCollectionTests
    {
        /// <summary>
        /// Test helper class that satisfies the generic constraints for AttachedCollection<T>.
        /// Inherits from BindableObject and implements IAttachedObject.
        /// </summary>
        private class TestAttachedObject : BindableObject, IAttachedObject
        {
            public void AttachTo(BindableObject bindable)
            {
            }

            public void DetachFrom(BindableObject bindable)
            {
            }
        }

        /// <summary>
        /// Tests that the AttachedCollection constructor properly initializes with a null list parameter.
        /// Should throw ArgumentNullException when null is passed to the base ObservableCollection constructor.
        /// </summary>
        [Fact]
        public void Constructor_WithNullList_ThrowsArgumentNullException()
        {
            // Arrange
            IList<TestAttachedObject> nullList = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AttachedCollection<TestAttachedObject>(nullList));
        }

        /// <summary>
        /// Tests that the AttachedCollection constructor properly initializes with an empty list.
        /// The resulting collection should be empty but not null.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyList_CreatesEmptyCollection()
        {
            // Arrange
            var emptyList = new List<TestAttachedObject>();

            // Act
            var collection = new AttachedCollection<TestAttachedObject>(emptyList);

            // Assert
            Assert.NotNull(collection);
            Assert.Empty(collection);
            Assert.Equal(0, collection.Count);
        }

        /// <summary>
        /// Tests that the AttachedCollection constructor properly initializes with a single item list.
        /// The resulting collection should contain exactly one item matching the input.
        /// </summary>
        [Fact]
        public void Constructor_WithSingleItemList_ContainsSingleItem()
        {
            // Arrange
            var testItem = new TestAttachedObject();
            var singleItemList = new List<TestAttachedObject> { testItem };

            // Act
            var collection = new AttachedCollection<TestAttachedObject>(singleItemList);

            // Assert
            Assert.NotNull(collection);
            Assert.Single(collection);
            Assert.Equal(1, collection.Count);
            Assert.Same(testItem, collection[0]);
        }

        /// <summary>
        /// Tests that the AttachedCollection constructor properly initializes with multiple items.
        /// The resulting collection should contain all items in the same order as the input list.
        /// </summary>
        [Fact]
        public void Constructor_WithMultipleItems_ContainsAllItemsInOrder()
        {
            // Arrange
            var item1 = new TestAttachedObject();
            var item2 = new TestAttachedObject();
            var item3 = new TestAttachedObject();
            var multipleItemsList = new List<TestAttachedObject> { item1, item2, item3 };

            // Act
            var collection = new AttachedCollection<TestAttachedObject>(multipleItemsList);

            // Assert
            Assert.NotNull(collection);
            Assert.Equal(3, collection.Count);
            Assert.Same(item1, collection[0]);
            Assert.Same(item2, collection[1]);
            Assert.Same(item3, collection[2]);
        }

        /// <summary>
        /// Tests that the AttachedCollection constructor works with different IList implementations.
        /// Verifies that any IList<T> implementation can be used, not just List<T>.
        /// </summary>
        [Fact]
        public void Constructor_WithArrayAsList_WorksCorrectly()
        {
            // Arrange
            var item1 = new TestAttachedObject();
            var item2 = new TestAttachedObject();
            TestAttachedObject[] arrayList = { item1, item2 };

            // Act
            var collection = new AttachedCollection<TestAttachedObject>(arrayList);

            // Assert
            Assert.NotNull(collection);
            Assert.Equal(2, collection.Count);
            Assert.Same(item1, collection[0]);
            Assert.Same(item2, collection[1]);
        }

        /// <summary>
        /// Tests that the AttachedCollection constructor handles duplicate items correctly.
        /// The collection should maintain all items including duplicates in their original order.
        /// </summary>
        [Fact]
        public void Constructor_WithDuplicateItems_MaintainsDuplicates()
        {
            // Arrange
            var item1 = new TestAttachedObject();
            var item2 = new TestAttachedObject();
            var listWithDuplicates = new List<TestAttachedObject> { item1, item2, item1, item2 };

            // Act
            var collection = new AttachedCollection<TestAttachedObject>(listWithDuplicates);

            // Assert
            Assert.NotNull(collection);
            Assert.Equal(4, collection.Count);
            Assert.Same(item1, collection[0]);
            Assert.Same(item2, collection[1]);
            Assert.Same(item1, collection[2]);
            Assert.Same(item2, collection[3]);
        }

        /// <summary>
        /// Test subclass of AttachedCollection to track OnAttachedTo calls.
        /// </summary>
        private class TestableAttachedCollection : AttachedCollection<TestAttachedObject>
        {
            public bool OnAttachedToCalled { get; private set; }
            public BindableObject LastAttachedBindable { get; private set; }

            protected override void OnAttachedTo(BindableObject bindable)
            {
                OnAttachedToCalled = true;
                LastAttachedBindable = bindable;
                base.OnAttachedTo(bindable);
            }
        }

        /// <summary>
        /// Tests that AttachTo throws ArgumentNullException when bindable parameter is null.
        /// This test exercises the null check validation logic.
        /// Expected result: ArgumentNullException with parameter name "bindable".
        /// </summary>
        [Fact]
        public void AttachTo_NullBindable_ThrowsArgumentNullException()
        {
            // Arrange
            var collection = new TestableAttachedCollection();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => collection.AttachTo(null));
            Assert.Equal("bindable", exception.ParamName);
        }

        /// <summary>
        /// Tests that AttachTo calls OnAttachedTo with the provided bindable object when bindable is not null.
        /// This test verifies the normal execution path and ensures the virtual method is called correctly.
        /// Expected result: OnAttachedTo is called with the same bindable object.
        /// </summary>
        [Fact]
        public void AttachTo_ValidBindable_CallsOnAttachedTo()
        {
            // Arrange
            var collection = new TestableAttachedCollection();
            var bindable = new BindableObject();

            // Act
            collection.AttachTo(bindable);

            // Assert
            Assert.True(collection.OnAttachedToCalled);
            Assert.Same(bindable, collection.LastAttachedBindable);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a valid AttachedCollection instance
        /// with proper initialization and empty collection state.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_CreatesEmptyCollection()
        {
            // Act
            var collection = new AttachedCollection<TestAttachedObject>();

            // Assert
            Assert.NotNull(collection);
            Assert.Empty(collection);
            Assert.Equal(0, collection.Count);
            Assert.IsAssignableFrom<ObservableCollection<TestAttachedObject>>(collection);
            Assert.IsAssignableFrom<ICollection<TestAttachedObject>>(collection);
            Assert.IsAssignableFrom<IAttachedObject>(collection);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an instance that implements
        /// all required interfaces properly.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_ImplementsRequiredInterfaces()
        {
            // Act
            var collection = new AttachedCollection<TestAttachedObject>();

            // Assert
            Assert.IsAssignableFrom<ICollection<TestAttachedObject>>(collection);
            Assert.IsAssignableFrom<IAttachedObject>(collection);
            Assert.IsAssignableFrom<ObservableCollection<TestAttachedObject>>(collection);
        }

        /// <summary>
        /// Tests that multiple instances created with the parameterless constructor
        /// are independent of each other.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_CreatesIndependentInstances()
        {
            // Act
            var collection1 = new AttachedCollection<TestAttachedObject>();
            var collection2 = new AttachedCollection<TestAttachedObject>();

            // Assert
            Assert.NotSame(collection1, collection2);
            Assert.Empty(collection1);
            Assert.Empty(collection2);
        }

    }
}