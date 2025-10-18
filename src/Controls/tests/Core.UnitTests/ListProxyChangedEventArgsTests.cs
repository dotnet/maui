#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the ListProxyChangedEventArgs class.
    /// </summary>
    public partial class ListProxyChangedEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor correctly assigns both null parameters to the properties.
        /// </summary>
        [Fact]
        public void Constructor_WithBothParametersNull_AssignsNullToProperties()
        {
            // Arrange
            IReadOnlyCollection<object> oldList = null;
            IReadOnlyCollection<object> newList = null;

            // Act
            var eventArgs = new ListProxyChangedEventArgs(oldList, newList);

            // Assert
            Assert.Null(eventArgs.OldList);
            Assert.Null(eventArgs.NewList);
        }

        /// <summary>
        /// Tests that the constructor correctly assigns parameters when oldList is null and newList is not null.
        /// </summary>
        [Fact]
        public void Constructor_WithOldListNullAndNewListNotNull_AssignsCorrectly()
        {
            // Arrange
            IReadOnlyCollection<object> oldList = null;
            IReadOnlyCollection<object> newList = new List<object> { "item1", "item2" };

            // Act
            var eventArgs = new ListProxyChangedEventArgs(oldList, newList);

            // Assert
            Assert.Null(eventArgs.OldList);
            Assert.Same(newList, eventArgs.NewList);
        }

        /// <summary>
        /// Tests that the constructor correctly assigns parameters when oldList is not null and newList is null.
        /// </summary>
        [Fact]
        public void Constructor_WithOldListNotNullAndNewListNull_AssignsCorrectly()
        {
            // Arrange
            IReadOnlyCollection<object> oldList = new List<object> { "oldItem" };
            IReadOnlyCollection<object> newList = null;

            // Act
            var eventArgs = new ListProxyChangedEventArgs(oldList, newList);

            // Assert
            Assert.Same(oldList, eventArgs.OldList);
            Assert.Null(eventArgs.NewList);
        }

        /// <summary>
        /// Tests that the constructor correctly assigns both non-null parameters to the properties.
        /// </summary>
        [Fact]
        public void Constructor_WithBothParametersNotNull_AssignsCorrectly()
        {
            // Arrange
            IReadOnlyCollection<object> oldList = new List<object> { "old1", "old2" };
            IReadOnlyCollection<object> newList = new List<object> { "new1", "new2", "new3" };

            // Act
            var eventArgs = new ListProxyChangedEventArgs(oldList, newList);

            // Assert
            Assert.Same(oldList, eventArgs.OldList);
            Assert.Same(newList, eventArgs.NewList);
        }

        /// <summary>
        /// Tests that the constructor correctly assigns empty collections to the properties.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyCollections_AssignsCorrectly()
        {
            // Arrange
            IReadOnlyCollection<object> oldList = new List<object>();
            IReadOnlyCollection<object> newList = new object[0];

            // Act
            var eventArgs = new ListProxyChangedEventArgs(oldList, newList);

            // Assert
            Assert.Same(oldList, eventArgs.OldList);
            Assert.Same(newList, eventArgs.NewList);
        }

        /// <summary>
        /// Tests that the constructor correctly assigns single-item collections to the properties.
        /// </summary>
        [Fact]
        public void Constructor_WithSingleItemCollections_AssignsCorrectly()
        {
            // Arrange
            IReadOnlyCollection<object> oldList = new List<object> { "singleOld" };
            IReadOnlyCollection<object> newList = new object[] { "singleNew" };

            // Act
            var eventArgs = new ListProxyChangedEventArgs(oldList, newList);

            // Assert
            Assert.Same(oldList, eventArgs.OldList);
            Assert.Same(newList, eventArgs.NewList);
        }

        /// <summary>
        /// Tests that the constructor works with different types of collections that implement IReadOnlyCollection.
        /// </summary>
        [Fact]
        public void Constructor_WithDifferentCollectionTypes_AssignsCorrectly()
        {
            // Arrange
            IReadOnlyCollection<object> oldList = new HashSet<object> { 1, 2, 3 };
            IReadOnlyCollection<object> newList = new object[] { "a", "b", "c", "d" };

            // Act
            var eventArgs = new ListProxyChangedEventArgs(oldList, newList);

            // Assert
            Assert.Same(oldList, eventArgs.OldList);
            Assert.Same(newList, eventArgs.NewList);
        }

        /// <summary>
        /// Tests that the constructor maintains reference equality for the same object passed to both parameters.
        /// </summary>
        [Fact]
        public void Constructor_WithSameObjectForBothParameters_MaintainsReferenceEquality()
        {
            // Arrange
            IReadOnlyCollection<object> sameList = new List<object> { "shared" };

            // Act
            var eventArgs = new ListProxyChangedEventArgs(sameList, sameList);

            // Assert
            Assert.Same(sameList, eventArgs.OldList);
            Assert.Same(sameList, eventArgs.NewList);
            Assert.Same(eventArgs.OldList, eventArgs.NewList);
        }

        /// <summary>
        /// Tests that the constructor inherits from EventArgs correctly.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstanceThatInheritsFromEventArgs()
        {
            // Arrange
            IReadOnlyCollection<object> oldList = new List<object>();
            IReadOnlyCollection<object> newList = new List<object>();

            // Act
            var eventArgs = new ListProxyChangedEventArgs(oldList, newList);

            // Assert
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }
    }
}
