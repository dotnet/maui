#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class PickerTests
    {
        /// <summary>
        /// Tests that MapItemsSource calls UpdateValue with the correct property name when valid parameters are provided.
        /// Input conditions: Valid IPickerHandler and IPicker instances.
        /// Expected result: UpdateValue is called with "Items" as the parameter.
        /// </summary>
        [Fact]
        public void MapItemsSource_ValidParameters_CallsUpdateValueWithItemsPropertyName()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            var view = Substitute.For<IPicker>();

            // Act
            Picker.MapItemsSource(handler, view);

            // Assert
            handler.Received(1).UpdateValue("Items");
        }

        /// <summary>
        /// Tests that MapItemsSource throws NullReferenceException when handler parameter is null.
        /// Input conditions: Null IPickerHandler, valid IPicker instance.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void MapItemsSource_NullHandler_ThrowsNullReferenceException()
        {
            // Arrange
            IPickerHandler handler = null;
            var view = Substitute.For<IPicker>();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Picker.MapItemsSource(handler, view));
        }

        /// <summary>
        /// Tests that MapItemsSource works correctly when view parameter is null since view is not used in the implementation.
        /// Input conditions: Valid IPickerHandler, null IPicker instance.
        /// Expected result: UpdateValue is called with "Items" as the parameter.
        /// </summary>
        [Fact]
        public void MapItemsSource_NullView_CallsUpdateValueWithItemsPropertyName()
        {
            // Arrange
            var handler = Substitute.For<IPickerHandler>();
            IPicker view = null;

            // Act
            Picker.MapItemsSource(handler, view);

            // Assert
            handler.Received(1).UpdateValue("Items");
        }

        /// <summary>
        /// Tests that MapItemsSource throws NullReferenceException when both parameters are null.
        /// Input conditions: Null IPickerHandler and null IPicker instances.
        /// Expected result: NullReferenceException is thrown due to null handler.
        /// </summary>
        [Fact]
        public void MapItemsSource_BothParametersNull_ThrowsNullReferenceException()
        {
            // Arrange
            IPickerHandler handler = null;
            IPicker view = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Picker.MapItemsSource(handler, view));
        }
    }
}
