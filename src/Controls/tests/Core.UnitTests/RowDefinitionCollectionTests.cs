#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for RowDefinitionCollection class.
    /// </summary>
    public sealed class RowDefinitionCollectionTests
    {
        /// <summary>
        /// Tests the internal constructor with null definitions list.
        /// Verifies that the constructor handles null definitions parameter correctly.
        /// Expected result: Constructor completes without throwing exception.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_NullDefinitions_CompletesSuccessfully(bool copy)
        {
            // Arrange
            List<RowDefinition> definitions = null;

            // Act & Assert
            var collection = new RowDefinitionCollection(definitions, copy);
            Assert.NotNull(collection);
        }

        /// <summary>
        /// Tests the internal constructor with empty definitions list.
        /// Verifies that the constructor handles empty list correctly.
        /// Expected result: Constructor completes and collection is empty.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_EmptyDefinitions_CompletesSuccessfully(bool copy)
        {
            // Arrange
            var definitions = new List<RowDefinition>();

            // Act
            var collection = new RowDefinitionCollection(definitions, copy);

            // Assert
            Assert.NotNull(collection);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests the internal constructor with single RowDefinition in list.
        /// Verifies that the constructor handles single-item list correctly.
        /// Expected result: Constructor completes and collection contains one item.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_SingleDefinition_CompletesSuccessfully(bool copy)
        {
            // Arrange
            var rowDefinition = new RowDefinition();
            var definitions = new List<RowDefinition> { rowDefinition };

            // Act
            var collection = new RowDefinitionCollection(definitions, copy);

            // Assert
            Assert.NotNull(collection);
            Assert.Single(collection);
            Assert.Equal(rowDefinition, collection[0]);
        }

        /// <summary>
        /// Tests the internal constructor with multiple RowDefinitions in list.
        /// Verifies that the constructor handles multi-item list correctly.
        /// Expected result: Constructor completes and collection contains all items.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_MultipleDefinitions_CompletesSuccessfully(bool copy)
        {
            // Arrange
            var rowDefinition1 = new RowDefinition();
            var rowDefinition2 = new RowDefinition(GridLength.Auto);
            var rowDefinition3 = new RowDefinition(new GridLength(100));
            var definitions = new List<RowDefinition> { rowDefinition1, rowDefinition2, rowDefinition3 };

            // Act
            var collection = new RowDefinitionCollection(definitions, copy);

            // Assert
            Assert.NotNull(collection);
            Assert.Equal(3, collection.Count);
            Assert.Equal(rowDefinition1, collection[0]);
            Assert.Equal(rowDefinition2, collection[1]);
            Assert.Equal(rowDefinition3, collection[2]);
        }

        /// <summary>
        /// Tests the internal constructor with RowDefinitions having different GridLength values.
        /// Verifies that the constructor preserves various GridLength configurations.
        /// Expected result: Constructor completes and all RowDefinitions maintain their properties.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_DefinitionsWithDifferentGridLengths_PreservesProperties(bool copy)
        {
            // Arrange
            var starDefinition = new RowDefinition(GridLength.Star);
            var autoDefinition = new RowDefinition(GridLength.Auto);
            var fixedDefinition = new RowDefinition(new GridLength(50, GridUnitType.Absolute));
            var definitions = new List<RowDefinition> { starDefinition, autoDefinition, fixedDefinition };

            // Act
            var collection = new RowDefinitionCollection(definitions, copy);

            // Assert
            Assert.NotNull(collection);
            Assert.Equal(3, collection.Count);
            Assert.Equal(GridLength.Star, collection[0].Height);
            Assert.Equal(GridLength.Auto, collection[1].Height);
            Assert.Equal(new GridLength(50, GridUnitType.Absolute), collection[2].Height);
        }
    }
}
