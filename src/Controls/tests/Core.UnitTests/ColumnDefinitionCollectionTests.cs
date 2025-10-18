#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for ColumnDefinitionCollection internal constructor.
    /// </summary>
    public sealed class ColumnDefinitionCollectionTests
    {
        /// <summary>
        /// Tests the internal constructor with a null definitions list and copy parameter set to false.
        /// Verifies that the constructor handles null input appropriately.
        /// </summary>
        [Fact]
        public void Constructor_NullDefinitionsListCopyFalse_HandlesNullInput()
        {
            // Arrange
            List<ColumnDefinition> definitions = null;
            bool copy = false;

            // Act & Assert
            var exception = Record.Exception(() => new ColumnDefinitionCollection(definitions, copy));

            // The behavior depends on the base class implementation
            // We verify that either it succeeds or throws an appropriate exception
            if (exception != null)
            {
                Assert.IsType<ArgumentNullException>(exception);
            }
        }

        /// <summary>
        /// Tests the internal constructor with a null definitions list and copy parameter set to true.
        /// Verifies that the constructor handles null input appropriately when copying is requested.
        /// </summary>
        [Fact]
        public void Constructor_NullDefinitionsListCopyTrue_HandlesNullInput()
        {
            // Arrange
            List<ColumnDefinition> definitions = null;
            bool copy = true;

            // Act & Assert
            var exception = Record.Exception(() => new ColumnDefinitionCollection(definitions, copy));

            // The behavior depends on the base class implementation
            // We verify that either it succeeds or throws an appropriate exception
            if (exception != null)
            {
                Assert.IsType<ArgumentNullException>(exception);
            }
        }

        /// <summary>
        /// Tests the internal constructor with an empty definitions list and copy parameter set to false.
        /// Verifies that the constructor properly initializes with an empty collection.
        /// </summary>
        [Fact]
        public void Constructor_EmptyDefinitionsListCopyFalse_InitializesSuccessfully()
        {
            // Arrange
            var definitions = new List<ColumnDefinition>();
            bool copy = false;

            // Act
            var collection = new ColumnDefinitionCollection(definitions, copy);

            // Assert
            Assert.NotNull(collection);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests the internal constructor with an empty definitions list and copy parameter set to true.
        /// Verifies that the constructor properly initializes with an empty collection when copying is requested.
        /// </summary>
        [Fact]
        public void Constructor_EmptyDefinitionsListCopyTrue_InitializesSuccessfully()
        {
            // Arrange
            var definitions = new List<ColumnDefinition>();
            bool copy = true;

            // Act
            var collection = new ColumnDefinitionCollection(definitions, copy);

            // Assert
            Assert.NotNull(collection);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests the internal constructor with a list containing valid ColumnDefinition objects and copy parameter set to false.
        /// Verifies that the constructor properly initializes with the provided definitions.
        /// </summary>
        [Fact]
        public void Constructor_ValidDefinitionsListCopyFalse_InitializesWithDefinitions()
        {
            // Arrange
            var columnDef1 = new ColumnDefinition();
            var columnDef2 = new ColumnDefinition(GridLength.Auto);
            var definitions = new List<ColumnDefinition> { columnDef1, columnDef2 };
            bool copy = false;

            // Act
            var collection = new ColumnDefinitionCollection(definitions, copy);

            // Assert
            Assert.NotNull(collection);
            Assert.Equal(2, collection.Count);
            Assert.Same(columnDef1, collection[0]);
            Assert.Same(columnDef2, collection[1]);
        }

        /// <summary>
        /// Tests the internal constructor with a list containing valid ColumnDefinition objects and copy parameter set to true.
        /// Verifies that the constructor properly initializes with the provided definitions when copying is requested.
        /// </summary>
        [Fact]
        public void Constructor_ValidDefinitionsListCopyTrue_InitializesWithDefinitions()
        {
            // Arrange
            var columnDef1 = new ColumnDefinition();
            var columnDef2 = new ColumnDefinition(GridLength.Auto);
            var definitions = new List<ColumnDefinition> { columnDef1, columnDef2 };
            bool copy = true;

            // Act
            var collection = new ColumnDefinitionCollection(definitions, copy);

            // Assert
            Assert.NotNull(collection);
            Assert.Equal(2, collection.Count);
            Assert.Same(columnDef1, collection[0]);
            Assert.Same(columnDef2, collection[1]);
        }

        /// <summary>
        /// Tests the internal constructor with a single ColumnDefinition in the list and copy parameter set to false.
        /// Verifies that the constructor handles single-item collections correctly.
        /// </summary>
        [Fact]
        public void Constructor_SingleDefinitionCopyFalse_InitializesWithSingleItem()
        {
            // Arrange
            var columnDef = new ColumnDefinition(new GridLength(100));
            var definitions = new List<ColumnDefinition> { columnDef };
            bool copy = false;

            // Act
            var collection = new ColumnDefinitionCollection(definitions, copy);

            // Assert
            Assert.NotNull(collection);
            Assert.Single(collection);
            Assert.Same(columnDef, collection[0]);
        }

        /// <summary>
        /// Tests the internal constructor with a single ColumnDefinition in the list and copy parameter set to true.
        /// Verifies that the constructor handles single-item collections correctly when copying is requested.
        /// </summary>
        [Fact]
        public void Constructor_SingleDefinitionCopyTrue_InitializesWithSingleItem()
        {
            // Arrange
            var columnDef = new ColumnDefinition(new GridLength(100));
            var definitions = new List<ColumnDefinition> { columnDef };
            bool copy = true;

            // Act
            var collection = new ColumnDefinitionCollection(definitions, copy);

            // Assert
            Assert.NotNull(collection);
            Assert.Single(collection);
            Assert.Same(columnDef, collection[0]);
        }
    }
}
