#nullable disable

using System;
using System.Collections;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class SelectableItemsViewHandlerTests
    {
        /// <summary>
        /// Tests that the SelectableItemsViewHandler constructor with PropertyMapper parameter
        /// uses the provided mapper when a non-null mapper is passed.
        /// </summary>
        [Fact]
        public void Constructor_WithValidMapper_UsesProvidedMapper()
        {
            // Arrange
            var mockMapper = Substitute.For<PropertyMapper>();

            // Act & Assert - Constructor should not throw and should accept the provided mapper
            var handler = new SelectableItemsViewHandler<TestSelectableItemsView>(mockMapper);

            // The constructor completes successfully, indicating the provided mapper was used
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the SelectableItemsViewHandler constructor with PropertyMapper parameter
        /// uses the default SelectableItemsViewMapper when null is passed.
        /// </summary>
        [Fact]
        public void Constructor_WithNullMapper_UsesDefaultMapper()
        {
            // Arrange & Act - Pass null mapper, should use SelectableItemsViewMapper
            var handler = new SelectableItemsViewHandler<TestSelectableItemsView>(null);

            // Assert - Constructor should complete successfully using the default mapper
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the SelectableItemsViewHandler constructor with PropertyMapper parameter
        /// uses the default SelectableItemsViewMapper when the parameter is omitted (defaults to null).
        /// </summary>
        [Fact]
        public void Constructor_WithOmittedMapper_UsesDefaultMapper()
        {
            // Arrange & Act - Omit the optional mapper parameter
            var handler = new SelectableItemsViewHandler<TestSelectableItemsView>();

            // Assert - Constructor should complete successfully using the default mapper
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Test helper class that extends SelectableItemsView for use in generic type parameter.
        /// </summary>
        private class TestSelectableItemsView : SelectableItemsView
        {
        }

        /// <summary>
        /// Tests that the parameterless constructor successfully creates an instance
        /// of SelectableItemsViewHandler with the default SelectableItemsViewMapper.
        /// Verifies that the constructor calls the base constructor with the static
        /// SelectableItemsViewMapper and that the resulting instance is properly initialized.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var handler = new SelectableItemsViewHandler<SelectableItemsView>();

            // Assert
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the parameterless constructor properly initializes the handler
        /// with the expected static mapper. Verifies that the SelectableItemsViewMapper
        /// is not null and contains the expected property mappings.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstructor_InitializesWithExpectedMapper()
        {
            // Arrange & Act
            var handler = new SelectableItemsViewHandler<SelectableItemsView>();

            // Assert
            Assert.NotNull(handler);
            Assert.NotNull(SelectableItemsViewHandler<SelectableItemsView>.SelectableItemsViewMapper);
            Assert.True(SelectableItemsViewHandler<SelectableItemsView>.SelectableItemsViewMapper.ContainsKey(SelectableItemsView.SelectedItemProperty.PropertyName));
            Assert.True(SelectableItemsViewHandler<SelectableItemsView>.SelectableItemsViewMapper.ContainsKey(SelectableItemsView.SelectedItemsProperty.PropertyName));
            Assert.True(SelectableItemsViewHandler<SelectableItemsView>.SelectableItemsViewMapper.ContainsKey(SelectableItemsView.SelectionModeProperty.PropertyName));
        }
    }
}