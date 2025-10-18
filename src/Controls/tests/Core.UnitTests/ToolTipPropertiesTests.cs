using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ToolTipPropertiesTests
    {
        /// <summary>
        /// Tests that SetText successfully calls SetValue on a valid BindableObject with a string value.
        /// Verifies the basic functionality of setting tooltip text.
        /// </summary>
        [Fact]
        public void SetText_WithValidBindableObjectAndStringValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();
            var value = "Test tooltip text";

            // Act
            ToolTipProperties.SetText(bindableObject, value);

            // Assert
            bindableObject.Received(1).SetValue(ToolTipProperties.TextProperty, value);
        }

        /// <summary>
        /// Tests that SetText throws NullReferenceException when bindable parameter is null.
        /// Verifies proper handling of null bindable object.
        /// </summary>
        [Fact]
        public void SetText_WithNullBindableObject_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindableObject = null;
            var value = "Test tooltip";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ToolTipProperties.SetText(bindableObject, value));
        }

        /// <summary>
        /// Tests that SetText accepts null values and passes them to SetValue.
        /// Verifies that null tooltip text can be set to clear tooltip.
        /// </summary>
        [Fact]
        public void SetText_WithNullValue_CallsSetValueWithNull()
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();
            object value = null;

            // Act
            ToolTipProperties.SetText(bindableObject, value);

            // Assert
            bindableObject.Received(1).SetValue(ToolTipProperties.TextProperty, null);
        }

        /// <summary>
        /// Tests that SetText handles various object types as values.
        /// Verifies that non-string objects can be used as tooltip values.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Normal text")]
        [InlineData("Text with special chars: !@#$%^&*()")]
        [InlineData("Very long text that exceeds normal tooltip length requirements and contains multiple sentences with various punctuation marks.")]
        public void SetText_WithVariousStringValues_CallsSetValueWithCorrectValue(string value)
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();

            // Act
            ToolTipProperties.SetText(bindableObject, value);

            // Assert
            bindableObject.Received(1).SetValue(ToolTipProperties.TextProperty, value);
        }

        /// <summary>
        /// Tests that SetText handles non-string object values correctly.
        /// Verifies that any object type can be used as tooltip value.
        /// </summary>
        [Theory]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void SetText_WithNonStringValues_CallsSetValueWithCorrectValue(object value)
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();

            // Act
            ToolTipProperties.SetText(bindableObject, value);

            // Assert
            bindableObject.Received(1).SetValue(ToolTipProperties.TextProperty, value);
        }

        /// <summary>
        /// Tests that GetToolTip throws NullReferenceException when bindable parameter is null.
        /// Input: null bindable object.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetToolTip_NullBindableObject_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ToolTipProperties.GetToolTip(bindable));
        }

        /// <summary>
        /// Tests that GetToolTip returns null when TextProperty is not set on the bindable object.
        /// Input: bindable object with TextProperty not set (IsSet returns false).
        /// Expected result: null is returned.
        /// </summary>
        [Fact]
        public void GetToolTip_TextPropertyNotSet_ReturnsNull()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.IsSet(ToolTipProperties.TextProperty).Returns(false);

            // Act
            var result = ToolTipProperties.GetToolTip(bindable);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetToolTip returns a ToolTip with correct content when TextProperty is set.
        /// Input: bindable object with TextProperty set and various content values.
        /// Expected result: ToolTip object with Content property set to the expected value.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Test tooltip text")]
        [InlineData("   ")]
        [InlineData("Special chars: !@#$%^&*()")]
        public void GetToolTip_TextPropertySet_ReturnsToolTipWithCorrectContent(object expectedContent)
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.IsSet(ToolTipProperties.TextProperty).Returns(true);
            bindable.GetValue(ToolTipProperties.TextProperty).Returns(expectedContent);

            // Act
            var result = ToolTipProperties.GetToolTip(bindable);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedContent, result.Content);
        }

        /// <summary>
        /// Tests that GetToolTip returns a ToolTip with object content when TextProperty contains non-string values.
        /// Input: bindable object with TextProperty set to various object types.
        /// Expected result: ToolTip object with Content property set to the object value.
        /// </summary>
        [Theory]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void GetToolTip_TextPropertySetWithObjectContent_ReturnsToolTipWithCorrectContent(object expectedContent)
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.IsSet(ToolTipProperties.TextProperty).Returns(true);
            bindable.GetValue(ToolTipProperties.TextProperty).Returns(expectedContent);

            // Act
            var result = ToolTipProperties.GetToolTip(bindable);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedContent, result.Content);
        }

        /// <summary>
        /// Tests that GetText returns the string value when a valid BindableObject has a text value set.
        /// Input: Valid BindableObject mock with a string value.
        /// Expected: Returns the string value cast to object.
        /// </summary>
        [Theory]
        [InlineData("Hello World")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Text with special characters: !@#$%^&*()")]
        [InlineData("Very long text that exceeds normal length expectations and contains multiple words and sentences to test boundary conditions.")]
        public void GetText_ValidBindableObjectWithValue_ReturnsValue(string expectedValue)
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(ToolTipProperties.TextProperty).Returns(expectedValue);

            // Act
            var result = ToolTipProperties.GetText(bindable);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetText returns null when a valid BindableObject has no text value set (default value).
        /// Input: Valid BindableObject mock returning null (default value).
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void GetText_ValidBindableObjectWithDefaultValue_ReturnsNull()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(ToolTipProperties.TextProperty).Returns((object)null);

            // Act
            var result = ToolTipProperties.GetText(bindable);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetText throws NullReferenceException when bindable parameter is null.
        /// Input: Null bindable parameter.
        /// Expected: Throws NullReferenceException.
        /// </summary>
        [Fact]
        public void GetText_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ToolTipProperties.GetText(bindable));
        }
    }
}