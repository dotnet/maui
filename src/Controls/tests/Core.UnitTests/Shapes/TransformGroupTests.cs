#nullable disable

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Shapes.UnitTests
{
    public sealed class TransformGroupTests
    {
        /// <summary>
        /// Tests that the Children property getter returns the initial TransformCollection created in the constructor.
        /// Verifies that the property is properly initialized and accessible after object creation.
        /// Expected result: Returns a non-null TransformCollection instance.
        /// </summary>
        [Fact]
        public void Children_GetAfterConstruction_ReturnsInitialTransformCollection()
        {
            // Arrange
            var transformGroup = new TransformGroup();

            // Act
            var children = transformGroup.Children;

            // Assert
            Assert.NotNull(children);
            Assert.IsType<TransformCollection>(children);
        }

        /// <summary>
        /// Tests that the Children property setter correctly assigns a new TransformCollection instance.
        /// Verifies that the setter properly stores the value using the underlying BindableProperty mechanism.
        /// Expected result: The getter returns the same TransformCollection instance that was set.
        /// </summary>
        [Fact]
        public void Children_SetNewTransformCollection_GetterReturnsSameInstance()
        {
            // Arrange
            var transformGroup = new TransformGroup();
            var newCollection = new TransformCollection();

            // Act
            transformGroup.Children = newCollection;

            // Assert
            Assert.Same(newCollection, transformGroup.Children);
        }

        /// <summary>
        /// Tests that the Children property setter accepts a null value.
        /// Verifies that the property can be set to null and the getter returns null accordingly.
        /// Expected result: The getter returns null after setting the property to null.
        /// </summary>
        [Fact]
        public void Children_SetNull_GetterReturnsNull()
        {
            // Arrange
            var transformGroup = new TransformGroup();

            // Act
            transformGroup.Children = null;

            // Assert
            Assert.Null(transformGroup.Children);
        }

        /// <summary>
        /// Tests that the Children property setter can replace an existing TransformCollection with a new one.
        /// Verifies that the property correctly overwrites the previous value with the new instance.
        /// Expected result: The getter returns the new TransformCollection instance, not the original one.
        /// </summary>
        [Fact]
        public void Children_SetMultipleDifferentCollections_GetterReturnsLatestCollection()
        {
            // Arrange
            var transformGroup = new TransformGroup();
            var firstCollection = new TransformCollection();
            var secondCollection = new TransformCollection();

            // Act
            transformGroup.Children = firstCollection;
            transformGroup.Children = secondCollection;

            // Assert
            Assert.Same(secondCollection, transformGroup.Children);
            Assert.NotSame(firstCollection, transformGroup.Children);
        }

        /// <summary>
        /// Tests that the Children property setter and getter work correctly with the same TransformCollection instance set multiple times.
        /// Verifies that setting the same instance multiple times maintains reference equality.
        /// Expected result: The getter consistently returns the same TransformCollection instance.
        /// </summary>
        [Fact]
        public void Children_SetSameCollectionMultipleTimes_GetterReturnsSameInstance()
        {
            // Arrange
            var transformGroup = new TransformGroup();
            var collection = new TransformCollection();

            // Act
            transformGroup.Children = collection;
            transformGroup.Children = collection;

            // Assert
            Assert.Same(collection, transformGroup.Children);
        }

        /// <summary>
        /// Tests that the TransformGroup constructor creates a valid instance with properly initialized Children property.
        /// Input: No parameters (parameterless constructor).
        /// Expected: TransformGroup instance is created with Children property initialized to a non-null TransformCollection.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_InitializesChildrenProperty()
        {
            // Act
            var transformGroup = new TransformGroup();

            // Assert
            Assert.NotNull(transformGroup);
            Assert.NotNull(transformGroup.Children);
            Assert.IsType<TransformCollection>(transformGroup.Children);
        }

        /// <summary>
        /// Tests that the TransformGroup constructor initializes Children as an empty collection.
        /// Input: No parameters (parameterless constructor).
        /// Expected: Children collection is empty and has Count of zero.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_InitializesChildrenAsEmptyCollection()
        {
            // Act
            var transformGroup = new TransformGroup();

            // Assert
            Assert.Empty(transformGroup.Children);
            Assert.Equal(0, transformGroup.Children.Count);
        }

        /// <summary>
        /// Tests that the TransformGroup constructor creates Children property that implements INotifyCollectionChanged.
        /// Input: No parameters (parameterless constructor).
        /// Expected: Children collection implements INotifyCollectionChanged interface.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_ChildrenImplementsINotifyCollectionChanged()
        {
            // Act
            var transformGroup = new TransformGroup();

            // Assert
            Assert.IsAssignableFrom<INotifyCollectionChanged>(transformGroup.Children);
        }

        /// <summary>
        /// Tests that the TransformGroup constructor creates Children property that implements ICollection.
        /// Input: No parameters (parameterless constructor).
        /// Expected: Children collection implements ICollection interface and supports collection operations.
        /// </summary>
        [Fact]
        public void Constructor_NoParameters_ChildrenImplementsICollection()
        {
            // Act
            var transformGroup = new TransformGroup();

            // Assert
            Assert.IsAssignableFrom<ICollection>(transformGroup.Children);
            Assert.False(((ICollection)transformGroup.Children).IsReadOnly);
        }

        /// <summary>
        /// Tests that multiple TransformGroup instances have separate Children collections.
        /// Input: Two separate constructor calls.
        /// Expected: Each TransformGroup has its own distinct Children collection instance.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_CreatesSeparateChildrenCollections()
        {
            // Act
            var transformGroup1 = new TransformGroup();
            var transformGroup2 = new TransformGroup();

            // Assert
            Assert.NotSame(transformGroup1.Children, transformGroup2.Children);
            Assert.NotEqual(transformGroup1.Children, transformGroup2.Children);
        }
    }
}