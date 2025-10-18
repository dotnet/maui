#nullable disable

using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class SelectionChangedEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when previousSelection parameter is null.
        /// This verifies the null check validation for the first parameter.
        /// </summary>
        [Fact]
        public void Constructor_PreviousSelectionNull_ThrowsArgumentNullException()
        {
            // Arrange
            IList<object> previousSelection = null;
            IList<object> currentSelection = new List<object>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new SelectionChangedEventArgs(previousSelection, currentSelection));
            Assert.Equal("previousSelection", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when currentSelection parameter is null.
        /// This verifies the null check validation for the second parameter.
        /// </summary>
        [Fact]
        public void Constructor_CurrentSelectionNull_ThrowsArgumentNullException()
        {
            // Arrange
            IList<object> previousSelection = new List<object>();
            IList<object> currentSelection = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new SelectionChangedEventArgs(previousSelection, currentSelection));
            Assert.Equal("currentSelection", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when both parameters are null.
        /// This verifies that the first null check (previousSelection) is evaluated first.
        /// </summary>
        [Fact]
        public void Constructor_BothParametersNull_ThrowsArgumentNullExceptionForPreviousSelection()
        {
            // Arrange
            IList<object> previousSelection = null;
            IList<object> currentSelection = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new SelectionChangedEventArgs(previousSelection, currentSelection));
            Assert.Equal("previousSelection", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor properly initializes properties with valid empty collections.
        /// This verifies that empty collections are handled correctly and properties are set.
        /// </summary>
        [Fact]
        public void Constructor_EmptyCollections_SetsPropertiesCorrectly()
        {
            // Arrange
            var previousSelection = new List<object>();
            var currentSelection = new List<object>();

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Assert
            Assert.NotNull(eventArgs.PreviousSelection);
            Assert.NotNull(eventArgs.CurrentSelection);
            Assert.Empty(eventArgs.PreviousSelection);
            Assert.Empty(eventArgs.CurrentSelection);
        }

        /// <summary>
        /// Tests that the constructor properly initializes properties with collections containing single items.
        /// This verifies that single-item collections are copied correctly to the properties.
        /// </summary>
        [Fact]
        public void Constructor_SingleItemCollections_SetsPropertiesCorrectly()
        {
            // Arrange
            var item1 = new object();
            var item2 = new object();
            var previousSelection = new List<object> { item1 };
            var currentSelection = new List<object> { item2 };

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Assert
            Assert.Single(eventArgs.PreviousSelection);
            Assert.Single(eventArgs.CurrentSelection);
            Assert.Same(item1, eventArgs.PreviousSelection[0]);
            Assert.Same(item2, eventArgs.CurrentSelection[0]);
        }

        /// <summary>
        /// Tests that the constructor properly initializes properties with collections containing multiple items.
        /// This verifies that multi-item collections are copied correctly and maintain order.
        /// </summary>
        [Fact]
        public void Constructor_MultipleItemCollections_SetsPropertiesCorrectly()
        {
            // Arrange
            var item1 = new object();
            var item2 = new object();
            var item3 = new object();
            var item4 = new object();
            var previousSelection = new List<object> { item1, item2 };
            var currentSelection = new List<object> { item3, item4 };

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Assert
            Assert.Equal(2, eventArgs.PreviousSelection.Count);
            Assert.Equal(2, eventArgs.CurrentSelection.Count);
            Assert.Same(item1, eventArgs.PreviousSelection[0]);
            Assert.Same(item2, eventArgs.PreviousSelection[1]);
            Assert.Same(item3, eventArgs.CurrentSelection[0]);
            Assert.Same(item4, eventArgs.CurrentSelection[1]);
        }

        /// <summary>
        /// Tests that the constructor creates independent copies of the input collections.
        /// This verifies that modifications to the original collections do not affect the event args properties.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_CreatesIndependentCopies()
        {
            // Arrange
            var item1 = new object();
            var item2 = new object();
            var previousSelection = new List<object> { item1 };
            var currentSelection = new List<object> { item2 };

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Modify original collections
            previousSelection.Add(new object());
            currentSelection.Clear();

            // Assert
            Assert.Single(eventArgs.PreviousSelection);
            Assert.Single(eventArgs.CurrentSelection);
            Assert.Same(item1, eventArgs.PreviousSelection[0]);
            Assert.Same(item2, eventArgs.CurrentSelection[0]);
        }

        /// <summary>
        /// Tests that the constructor works with different IList implementations.
        /// This verifies that any IList&lt;object&gt; implementation is accepted and copied correctly.
        /// </summary>
        [Fact]
        public void Constructor_DifferentIListImplementations_SetsPropertiesCorrectly()
        {
            // Arrange
            var item1 = new object();
            var item2 = new object();
            IList<object> previousSelection = new object[] { item1 };
            IList<object> currentSelection = new List<object> { item2 };

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Assert
            Assert.Single(eventArgs.PreviousSelection);
            Assert.Single(eventArgs.CurrentSelection);
            Assert.Same(item1, eventArgs.PreviousSelection[0]);
            Assert.Same(item2, eventArgs.CurrentSelection[0]);
        }

        /// <summary>
        /// Tests that the constructor handles mixed scenarios with one empty and one non-empty collection.
        /// This verifies that the constructor correctly handles asymmetric input scenarios.
        /// </summary>
        [Fact]
        public void Constructor_MixedEmptyAndNonEmpty_SetsPropertiesCorrectly()
        {
            // Arrange
            var item = new object();
            var previousSelection = new List<object>();
            var currentSelection = new List<object> { item };

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Assert
            Assert.Empty(eventArgs.PreviousSelection);
            Assert.Single(eventArgs.CurrentSelection);
            Assert.Same(item, eventArgs.CurrentSelection[0]);
        }

        /// <summary>
        /// Tests the constructor when both previousSelection and currentSelection parameters are null.
        /// Should set both PreviousSelection and CurrentSelection properties to empty collections.
        /// </summary>
        [Fact]
        public void Constructor_BothParametersNull_SetsBothPropertiesToEmptyCollections()
        {
            // Arrange
            object previousSelection = null;
            object currentSelection = null;

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Assert
            Assert.NotNull(eventArgs.PreviousSelection);
            Assert.NotNull(eventArgs.CurrentSelection);
            Assert.Empty(eventArgs.PreviousSelection);
            Assert.Empty(eventArgs.CurrentSelection);
        }

        /// <summary>
        /// Tests the constructor when previousSelection is null and currentSelection is not null.
        /// Should set PreviousSelection to empty collection and CurrentSelection to single-item collection.
        /// </summary>
        [Fact]
        public void Constructor_PreviousSelectionNull_SetsPreviousToEmptyCurrentToSingleItem()
        {
            // Arrange
            object previousSelection = null;
            object currentSelection = "test item";

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Assert
            Assert.NotNull(eventArgs.PreviousSelection);
            Assert.Empty(eventArgs.PreviousSelection);
            Assert.NotNull(eventArgs.CurrentSelection);
            Assert.Single(eventArgs.CurrentSelection);
            Assert.Equal(currentSelection, eventArgs.CurrentSelection[0]);
        }

        /// <summary>
        /// Tests the constructor when currentSelection is null and previousSelection is not null.
        /// Should set CurrentSelection to empty collection and PreviousSelection to single-item collection.
        /// </summary>
        [Fact]
        public void Constructor_CurrentSelectionNull_SetsCurrentToEmptyPreviousToSingleItem()
        {
            // Arrange
            object previousSelection = "previous item";
            object currentSelection = null;

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Assert
            Assert.NotNull(eventArgs.PreviousSelection);
            Assert.Single(eventArgs.PreviousSelection);
            Assert.Equal(previousSelection, eventArgs.PreviousSelection[0]);
            Assert.NotNull(eventArgs.CurrentSelection);
            Assert.Empty(eventArgs.CurrentSelection);
        }

        /// <summary>
        /// Tests the constructor when both previousSelection and currentSelection are not null.
        /// Should set both properties to single-item collections containing the respective parameters.
        /// </summary>
        [Fact]
        public void Constructor_BothParametersNotNull_SetsBothPropertiesToSingleItemCollections()
        {
            // Arrange
            object previousSelection = "previous item";
            object currentSelection = "current item";

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Assert
            Assert.NotNull(eventArgs.PreviousSelection);
            Assert.Single(eventArgs.PreviousSelection);
            Assert.Equal(previousSelection, eventArgs.PreviousSelection[0]);
            Assert.NotNull(eventArgs.CurrentSelection);
            Assert.Single(eventArgs.CurrentSelection);
            Assert.Equal(currentSelection, eventArgs.CurrentSelection[0]);
        }

        /// <summary>
        /// Tests the constructor with different object types to ensure type safety.
        /// Should handle various object types correctly in the collections.
        /// </summary>
        [Theory]
        [InlineData(42, "string")]
        [InlineData(true, 3.14)]
        [InlineData('c', null)]
        [InlineData(null, new int[] { 1, 2, 3 })]
        public void Constructor_WithDifferentObjectTypes_CreatesCorrectCollections(object previous, object current)
        {
            // Act
            var eventArgs = new SelectionChangedEventArgs(previous, current);

            // Assert
            if (previous == null)
            {
                Assert.Empty(eventArgs.PreviousSelection);
            }
            else
            {
                Assert.Single(eventArgs.PreviousSelection);
                Assert.Equal(previous, eventArgs.PreviousSelection[0]);
            }

            if (current == null)
            {
                Assert.Empty(eventArgs.CurrentSelection);
            }
            else
            {
                Assert.Single(eventArgs.CurrentSelection);
                Assert.Equal(current, eventArgs.CurrentSelection[0]);
            }
        }

        /// <summary>
        /// Tests that the constructor creates new collections and doesn't share references.
        /// Should ensure that PreviousSelection and CurrentSelection are independent collections.
        /// </summary>
        [Fact]
        public void Constructor_CreatesIndependentCollections_PropertiesAreNotSameReference()
        {
            // Arrange
            object previousSelection = "previous";
            object currentSelection = "current";

            // Act
            var eventArgs = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Assert
            Assert.NotSame(eventArgs.PreviousSelection, eventArgs.CurrentSelection);
        }

        /// <summary>
        /// Tests that when parameters are null, both properties reference the same empty collection instance.
        /// Should verify memory efficiency by reusing the static empty collection.
        /// </summary>
        [Fact]
        public void Constructor_BothParametersNull_PropertiesReferenceSameEmptyCollection()
        {
            // Arrange & Act
            var eventArgs = new SelectionChangedEventArgs(null, null);

            // Assert
            Assert.Same(eventArgs.PreviousSelection, eventArgs.CurrentSelection);
            Assert.Empty(eventArgs.PreviousSelection);
            Assert.Empty(eventArgs.CurrentSelection);
        }
    }
}