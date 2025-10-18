#nullable disable

using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ItemsViewConstructorTests : BaseTestFixture
    {
        public ItemsViewConstructorTests()
        {
            DeviceDisplay.SetCurrent(new MockDeviceDisplay());
        }

        /// <summary>
        /// Tests that the ItemsView constructor properly initializes the TemplatedItems property.
        /// Verifies that TemplatedItems is not null and is of the correct type after construction.
        /// </summary>
        [Fact]
        public void Constructor_InitializesTemplatedItems_TemplatedItemsIsNotNullAndCorrectType()
        {
            // Arrange & Act
            var itemsView = new TestItemsView();

            // Assert
            Assert.NotNull(itemsView.TemplatedItems);
            Assert.IsType<TemplatedItemsList<TestItemsView, Label>>(itemsView.TemplatedItems);
        }

        /// <summary>
        /// Tests that the ItemsView constructor creates TemplatedItems with proper initialization.
        /// Verifies that the TemplatedItems property has expected initial state.
        /// </summary>
        [Fact]
        public void Constructor_InitializesTemplatedItems_HasExpectedInitialState()
        {
            // Arrange & Act
            var itemsView = new TestItemsView();

            // Assert  
            Assert.NotNull(itemsView.TemplatedItems);
            Assert.NotNull(itemsView.TemplatedItems.ListProxy);
        }

        /// <summary>
        /// Concrete test implementation of ItemsView for testing purposes.
        /// Uses Label as TVisual since it's a commonly used BindableObject derivative.
        /// </summary>
        private class TestItemsView : ItemsView<Label>
        {
            protected override Label CreateDefault(object item)
            {
                return new Label { Text = item?.ToString() };
            }
        }
    }
}