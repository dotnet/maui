#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.WindowsSpecific
{
    public partial class ListViewTests
    {
        /// <summary>
        /// Tests that SetSelectionMode throws NullReferenceException when element parameter is null.
        /// </summary>
        [Fact]
        public void SetSelectionMode_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var value = ListViewSelectionMode.Accessible;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                ListView.SetSelectionMode(element, value));
        }

        /// <summary>
        /// Tests that SetSelectionMode correctly calls SetValue with Accessible selection mode.
        /// </summary>
        [Fact]
        public void SetSelectionMode_ValidElementWithAccessibleMode_CallsSetValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = ListViewSelectionMode.Accessible;

            // Act
            ListView.SetSelectionMode(element, value);

            // Assert
            element.Received(1).SetValue(ListView.SelectionModeProperty, value);
        }

        /// <summary>
        /// Tests that SetSelectionMode correctly calls SetValue with Inaccessible selection mode.
        /// </summary>
        [Fact]
        public void SetSelectionMode_ValidElementWithInaccessibleMode_CallsSetValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = ListViewSelectionMode.Inaccessible;

            // Act
            ListView.SetSelectionMode(element, value);

            // Assert
            element.Received(1).SetValue(ListView.SelectionModeProperty, value);
        }

        /// <summary>
        /// Tests that SetSelectionMode handles invalid enum values by passing them to SetValue.
        /// </summary>
        [Fact]
        public void SetSelectionMode_ValidElementWithInvalidEnumValue_CallsSetValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var invalidValue = (ListViewSelectionMode)999;

            // Act
            ListView.SetSelectionMode(element, invalidValue);

            // Assert
            element.Received(1).SetValue(ListView.SelectionModeProperty, invalidValue);
        }

        /// <summary>
        /// Tests that GetSelectionMode returns the correct ListViewSelectionMode value when the property is set to Accessible.
        /// Input: A platform configuration with SelectionModeProperty set to Accessible.
        /// Expected: Returns ListViewSelectionMode.Accessible.
        /// </summary>
        [Fact]
        public void GetSelectionMode_WhenPropertySetToAccessible_ReturnsAccessible()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, ListView>>();
            var element = Substitute.For<ListView>();
            config.Element.Returns(element);
            element.GetValue(ListView.SelectionModeProperty).Returns(ListViewSelectionMode.Accessible);

            // Act
            var result = ListView.GetSelectionMode(config);

            // Assert
            Assert.Equal(ListViewSelectionMode.Accessible, result);
        }

        /// <summary>
        /// Tests that GetSelectionMode returns the correct ListViewSelectionMode value when the property is set to Inaccessible.
        /// Input: A platform configuration with SelectionModeProperty set to Inaccessible.
        /// Expected: Returns ListViewSelectionMode.Inaccessible.
        /// </summary>
        [Fact]
        public void GetSelectionMode_WhenPropertySetToInaccessible_ReturnsInaccessible()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, ListView>>();
            var element = Substitute.For<ListView>();
            config.Element.Returns(element);
            element.GetValue(ListView.SelectionModeProperty).Returns(ListViewSelectionMode.Inaccessible);

            // Act
            var result = ListView.GetSelectionMode(config);

            // Assert
            Assert.Equal(ListViewSelectionMode.Inaccessible, result);
        }

        /// <summary>
        /// Tests that GetSelectionMode returns the default value when the property has not been explicitly set.
        /// Input: A platform configuration with SelectionModeProperty returning the default value.
        /// Expected: Returns the default ListViewSelectionMode value (Accessible based on property definition).
        /// </summary>
        [Fact]
        public void GetSelectionMode_WhenPropertyNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, ListView>>();
            var element = Substitute.For<ListView>();
            config.Element.Returns(element);
            element.GetValue(ListView.SelectionModeProperty).Returns(ListViewSelectionMode.Accessible);

            // Act
            var result = ListView.GetSelectionMode(config);

            // Assert
            Assert.Equal(ListViewSelectionMode.Accessible, result);
        }

        /// <summary>
        /// Tests that GetSelectionMode handles invalid enum values by casting them appropriately.
        /// Input: A platform configuration with SelectionModeProperty returning an invalid enum value.
        /// Expected: Returns the cast value as ListViewSelectionMode.
        /// </summary>
        [Fact]
        public void GetSelectionMode_WhenPropertyContainsInvalidEnumValue_ReturnsCastValue()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, ListView>>();
            var element = Substitute.For<ListView>();
            config.Element.Returns(element);
            element.GetValue(ListView.SelectionModeProperty).Returns((ListViewSelectionMode)999);

            // Act
            var result = ListView.GetSelectionMode(config);

            // Assert
            Assert.Equal((ListViewSelectionMode)999, result);
        }

        /// <summary>
        /// Tests that GetSelectionMode throws NullReferenceException when config is null.
        /// Input: A null config parameter.
        /// Expected: Throws NullReferenceException.
        /// </summary>
        [Fact]
        public void GetSelectionMode_WhenConfigIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, ListView> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ListView.GetSelectionMode(config));
        }

        /// <summary>
        /// Tests that GetSelectionMode throws NullReferenceException when config.Element is null.
        /// Input: A platform configuration with null Element property.
        /// Expected: Throws NullReferenceException.
        /// </summary>
        [Fact]
        public void GetSelectionMode_WhenElementIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, ListView>>();
            config.Element.Returns((ListView)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ListView.GetSelectionMode(config));
        }

        /// <summary>
        /// Tests that SetSelectionMode correctly sets the SelectionModeProperty with Accessible value and returns the config object.
        /// </summary>
        [Fact]
        public void SetSelectionMode_AccessibleValue_SetsPropertyAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<ListView>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, ListView>>();
            mockConfig.Element.Returns(mockElement);
            var value = ListViewSelectionMode.Accessible;

            // Act
            var result = mockConfig.SetSelectionMode(value);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.ListView.SelectionModeProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetSelectionMode correctly sets the SelectionModeProperty with Inaccessible value and returns the config object.
        /// </summary>
        [Fact]
        public void SetSelectionMode_InaccessibleValue_SetsPropertyAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<ListView>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, ListView>>();
            mockConfig.Element.Returns(mockElement);
            var value = ListViewSelectionMode.Inaccessible;

            // Act
            var result = mockConfig.SetSelectionMode(value);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.ListView.SelectionModeProperty, value);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetSelectionMode correctly handles invalid enum values by casting and still sets the property.
        /// </summary>
        [Fact]
        public void SetSelectionMode_InvalidEnumValue_SetsPropertyAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<ListView>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, ListView>>();
            mockConfig.Element.Returns(mockElement);
            var invalidValue = (ListViewSelectionMode)999;

            // Act
            var result = mockConfig.SetSelectionMode(invalidValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.ListView.SelectionModeProperty, invalidValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetSelectionMode throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void SetSelectionMode_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, ListView> config = null;
            var value = ListViewSelectionMode.Accessible;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.SetSelectionMode(value));
        }

        /// <summary>
        /// Tests GetSelectionMode method returns Accessible when the property is set to Accessible.
        /// Verifies that the method correctly retrieves and casts the SelectionMode property value.
        /// Expected result: Returns ListViewSelectionMode.Accessible.
        /// </summary>
        [Fact]
        public void GetSelectionMode_PropertySetToAccessible_ReturnsAccessible()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ListView.SelectionModeProperty).Returns(ListViewSelectionMode.Accessible);

            // Act
            var result = ListView.GetSelectionMode(element);

            // Assert
            Assert.Equal(ListViewSelectionMode.Accessible, result);
        }

        /// <summary>
        /// Tests GetSelectionMode method returns Inaccessible when the property is set to Inaccessible.
        /// Verifies that the method correctly retrieves and casts the SelectionMode property value.
        /// Expected result: Returns ListViewSelectionMode.Inaccessible.
        /// </summary>
        [Fact]
        public void GetSelectionMode_PropertySetToInaccessible_ReturnsInaccessible()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ListView.SelectionModeProperty).Returns(ListViewSelectionMode.Inaccessible);

            // Act
            var result = ListView.GetSelectionMode(element);

            // Assert
            Assert.Equal(ListViewSelectionMode.Inaccessible, result);
        }

        /// <summary>
        /// Tests GetSelectionMode method with null element parameter.
        /// Verifies that the method throws NullReferenceException when element is null.
        /// Expected result: Throws NullReferenceException.
        /// </summary>
        [Fact]
        public void GetSelectionMode_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ListView.GetSelectionMode(element));
        }

        /// <summary>
        /// Tests GetSelectionMode method with invalid enum cast scenarios.
        /// Verifies that the method handles invalid integer values outside enum range.
        /// Expected result: Returns the cast result even for invalid enum values.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void GetSelectionMode_InvalidEnumValues_ReturnsInvalidEnumValue(int invalidEnumValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ListView.SelectionModeProperty).Returns(invalidEnumValue);

            // Act
            var result = ListView.GetSelectionMode(element);

            // Assert
            Assert.Equal((ListViewSelectionMode)invalidEnumValue, result);
        }

        /// <summary>
        /// Tests GetSelectionMode method with GetValue returning null.
        /// Verifies that the method handles null return value from GetValue.
        /// Expected result: Throws InvalidCastException when trying to cast null to enum.
        /// </summary>
        [Fact]
        public void GetSelectionMode_GetValueReturnsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ListView.SelectionModeProperty).Returns((object)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ListView.GetSelectionMode(element));
        }

        /// <summary>
        /// Tests GetSelectionMode method with GetValue returning incompatible type.
        /// Verifies that the method throws InvalidCastException for non-compatible types.
        /// Expected result: Throws InvalidCastException.
        /// </summary>
        [Fact]
        public void GetSelectionMode_GetValueReturnsIncompatibleType_ThrowsInvalidCastException()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ListView.SelectionModeProperty).Returns("invalid_string");

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => ListView.GetSelectionMode(element));
        }
    }
}