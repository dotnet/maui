#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the AutomationProperties class.
    /// </summary>
    public class AutomationPropertiesTests
    {
        /// <summary>
        /// Tests that GetIsInAccessibleTree throws NullReferenceException when bindable parameter is null.
        /// </summary>
        [Fact]
        public void GetIsInAccessibleTree_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AutomationProperties.GetIsInAccessibleTree(bindable));
        }

        /// <summary>
        /// Tests that GetIsInAccessibleTree returns null when GetValue returns null.
        /// </summary>
        [Fact]
        public void GetIsInAccessibleTree_GetValueReturnsNull_ReturnsNull()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.IsInAccessibleTreeProperty).Returns(null);

            // Act
            var result = AutomationProperties.GetIsInAccessibleTree(bindable);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetIsInAccessibleTree returns true when GetValue returns true.
        /// </summary>
        [Fact]
        public void GetIsInAccessibleTree_GetValueReturnsTrue_ReturnsTrue()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.IsInAccessibleTreeProperty).Returns(true);

            // Act
            var result = AutomationProperties.GetIsInAccessibleTree(bindable);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetIsInAccessibleTree returns false when GetValue returns false.
        /// </summary>
        [Fact]
        public void GetIsInAccessibleTree_GetValueReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.IsInAccessibleTreeProperty).Returns(false);

            // Act
            var result = AutomationProperties.GetIsInAccessibleTree(bindable);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetIsInAccessibleTree passes the correct property to GetValue method.
        /// </summary>
        [Fact]
        public void GetIsInAccessibleTree_ValidBindable_PassesCorrectPropertyToGetValue()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(Arg.Any<BindableProperty>()).Returns(true);

            // Act
            AutomationProperties.GetIsInAccessibleTree(bindable);

            // Assert
            bindable.Received(1).GetValue(AutomationProperties.IsInAccessibleTreeProperty);
        }

        /// <summary>
        /// Tests that GetExcludedWithChildren throws ArgumentNullException when bindable parameter is null.
        /// Input: null bindable object
        /// Expected: NullReferenceException is thrown
        /// </summary>
        [Fact]
        public void GetExcludedWithChildren_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AutomationProperties.GetExcludedWithChildren(bindable));
        }

        /// <summary>
        /// Tests that GetExcludedWithChildren returns null when the property value is null.
        /// Input: valid bindable object with null ExcludedWithChildren value
        /// Expected: null is returned
        /// </summary>
        [Fact]
        public void GetExcludedWithChildren_ValidBindableWithNullValue_ReturnsNull()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.ExcludedWithChildrenProperty).Returns((bool?)null);

            // Act
            var result = AutomationProperties.GetExcludedWithChildren(bindable);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetExcludedWithChildren returns true when the property value is true.
        /// Input: valid bindable object with ExcludedWithChildren value set to true
        /// Expected: true is returned
        /// </summary>
        [Fact]
        public void GetExcludedWithChildren_ValidBindableWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.ExcludedWithChildrenProperty).Returns(true);

            // Act
            var result = AutomationProperties.GetExcludedWithChildren(bindable);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetExcludedWithChildren returns false when the property value is false.
        /// Input: valid bindable object with ExcludedWithChildren value set to false
        /// Expected: false is returned
        /// </summary>
        [Fact]
        public void GetExcludedWithChildren_ValidBindableWithFalseValue_ReturnsFalse()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.ExcludedWithChildrenProperty).Returns(false);

            // Act
            var result = AutomationProperties.GetExcludedWithChildren(bindable);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetLabeledBy throws NullReferenceException when bindable parameter is null.
        /// </summary>
        [Fact]
        public void GetLabeledBy_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AutomationProperties.GetLabeledBy(bindable));
        }

        /// <summary>
        /// Tests that GetLabeledBy returns null when the bindable object's LabeledByProperty value is null.
        /// </summary>
        [Fact]
        public void GetLabeledBy_PropertyValueIsNull_ReturnsNull()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.LabeledByProperty).Returns((VisualElement)null);

            // Act
            var result = AutomationProperties.GetLabeledBy(bindable);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetLabeledBy returns the VisualElement when the bindable object's LabeledByProperty has a valid VisualElement value.
        /// </summary>
        [Fact]
        public void GetLabeledBy_PropertyValueIsValidVisualElement_ReturnsVisualElement()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            var expectedVisualElement = Substitute.For<VisualElement>();
            bindable.GetValue(AutomationProperties.LabeledByProperty).Returns(expectedVisualElement);

            // Act
            var result = AutomationProperties.GetLabeledBy(bindable);

            // Assert
            Assert.Same(expectedVisualElement, result);
        }

        /// <summary>
        /// Tests that GetLabeledBy throws InvalidCastException when the bindable object's LabeledByProperty contains a non-VisualElement value.
        /// </summary>
        [Fact]
        public void GetLabeledBy_PropertyValueIsNotVisualElement_ThrowsInvalidCastException()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            var nonVisualElementValue = new object();
            bindable.GetValue(AutomationProperties.LabeledByProperty).Returns(nonVisualElementValue);

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => AutomationProperties.GetLabeledBy(bindable));
        }

        /// <summary>
        /// Tests that GetName returns the correct string value when bindable object has a name set.
        /// </summary>
        [Fact]
        public void GetName_WithValidBindableObjectAndNameSet_ReturnsCorrectName()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            const string expectedName = "Test Name";
            bindable.GetValue(AutomationProperties.NameProperty).Returns(expectedName);

            // Act
            var result = AutomationProperties.GetName(bindable);

            // Assert
            Assert.Equal(expectedName, result);
        }

        /// <summary>
        /// Tests that GetName returns null when bindable object has null name value.
        /// </summary>
        [Fact]
        public void GetName_WithValidBindableObjectAndNullName_ReturnsNull()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.NameProperty).Returns((string)null);

            // Act
            var result = AutomationProperties.GetName(bindable);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetName returns empty string when bindable object has empty string name value.
        /// </summary>
        [Fact]
        public void GetName_WithValidBindableObjectAndEmptyName_ReturnsEmptyString()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.NameProperty).Returns(string.Empty);

            // Act
            var result = AutomationProperties.GetName(bindable);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that GetName returns whitespace string when bindable object has whitespace name value.
        /// </summary>
        [Fact]
        public void GetName_WithValidBindableObjectAndWhitespaceName_ReturnsWhitespace()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            const string whitespaceValue = "   \t\n\r   ";
            bindable.GetValue(AutomationProperties.NameProperty).Returns(whitespaceValue);

            // Act
            var result = AutomationProperties.GetName(bindable);

            // Assert
            Assert.Equal(whitespaceValue, result);
        }

        /// <summary>
        /// Tests that GetName returns very long string when bindable object has very long name value.
        /// </summary>
        [Fact]
        public void GetName_WithValidBindableObjectAndVeryLongName_ReturnsLongString()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            var longName = new string('A', 10000);
            bindable.GetValue(AutomationProperties.NameProperty).Returns(longName);

            // Act
            var result = AutomationProperties.GetName(bindable);

            // Assert
            Assert.Equal(longName, result);
        }

        /// <summary>
        /// Tests that GetName returns string with special characters when bindable object has special character name value.
        /// </summary>
        [Fact]
        public void GetName_WithValidBindableObjectAndSpecialCharactersName_ReturnsSpecialCharacters()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            const string specialCharsName = "Test\0Name\u0001With\u001FSpecial\u007FChars";
            bindable.GetValue(AutomationProperties.NameProperty).Returns(specialCharsName);

            // Act
            var result = AutomationProperties.GetName(bindable);

            // Assert
            Assert.Equal(specialCharsName, result);
        }

        /// <summary>
        /// Tests that GetName throws ArgumentNullException when bindable parameter is null.
        /// </summary>
        [Fact]
        public void GetName_WithNullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AutomationProperties.GetName(bindable));
        }

        /// <summary>
        /// Tests SetHelpText method with various string values to ensure it properly calls SetValue with the correct parameters.
        /// </summary>
        /// <param name="value">The help text value to set.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Sample help text")]
        [InlineData("Help text with special characters: !@#$%^&*()")]
        [InlineData("Very long help text that exceeds normal expectations and contains multiple sentences to test the behavior with longer strings that might be used in real-world scenarios.")]
        public void SetHelpText_WithValidBindableAndVariousValues_CallsSetValueWithCorrectParameters(string value)
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();

            // Act
            AutomationProperties.SetHelpText(bindable, value);

            // Assert
            bindable.Received(1).SetValue(AutomationProperties.HelpTextProperty, value);
        }

        /// <summary>
        /// Tests SetHelpText method with null bindable object to ensure it throws NullReferenceException.
        /// </summary>
        [Fact]
        public void SetHelpText_WithNullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;
            string value = "test value";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AutomationProperties.SetHelpText(bindable, value));
        }

        /// <summary>
        /// Tests SetHelpText method with unicode characters to ensure proper handling of special string content.
        /// </summary>
        [Fact]
        public void SetHelpText_WithUnicodeCharacters_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            string unicodeValue = "Unicode test: 你好世界 🌍 ñáéíóú";

            // Act
            AutomationProperties.SetHelpText(bindable, unicodeValue);

            // Assert
            bindable.Received(1).SetValue(AutomationProperties.HelpTextProperty, unicodeValue);
        }

        /// <summary>
        /// Tests SetHelpText method with control characters to ensure proper handling of special string content.
        /// </summary>
        [Fact]
        public void SetHelpText_WithControlCharacters_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            string controlCharValue = "Text with\ttab\nand\rnewlines";

            // Act
            AutomationProperties.SetHelpText(bindable, controlCharValue);

            // Assert
            bindable.Received(1).SetValue(AutomationProperties.HelpTextProperty, controlCharValue);
        }

        /// <summary>
        /// Tests that SetIsInAccessibleTree throws NullReferenceException when bindable parameter is null.
        /// Verifies that the method properly validates the bindable parameter.
        /// </summary>
        [Fact]
        public void SetIsInAccessibleTree_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;
            bool? value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AutomationProperties.SetIsInAccessibleTree(bindable, value));
        }

        /// <summary>
        /// Tests that SetIsInAccessibleTree calls SetValue with the correct parameters for all valid bool? values.
        /// Verifies that the method properly passes the IsInAccessibleTreeProperty and value to the bindable object.
        /// </summary>
        /// <param name="value">The bool? value to test (true, false, or null)</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public void SetIsInAccessibleTree_WithValidValue_CallsSetValueWithCorrectParameters(bool? value)
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();

            // Act
            AutomationProperties.SetIsInAccessibleTree(bindable, value);

            // Assert
            bindable.Received(1).SetValue(AutomationProperties.IsInAccessibleTreeProperty, value);
        }

        /// <summary>
        /// Tests that SetExcludedWithChildren calls SetValue with correct parameters when bindable object is valid and value is true.
        /// </summary>
        [Fact]
        public void SetExcludedWithChildren_ValidBindableObjectAndTrueValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bool? value = true;

            // Act
            AutomationProperties.SetExcludedWithChildren(bindable, value);

            // Assert
            bindable.Received(1).SetValue(AutomationProperties.ExcludedWithChildrenProperty, value);
        }

        /// <summary>
        /// Tests that SetExcludedWithChildren calls SetValue with correct parameters when bindable object is valid and value is false.
        /// </summary>
        [Fact]
        public void SetExcludedWithChildren_ValidBindableObjectAndFalseValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bool? value = false;

            // Act
            AutomationProperties.SetExcludedWithChildren(bindable, value);

            // Assert
            bindable.Received(1).SetValue(AutomationProperties.ExcludedWithChildrenProperty, value);
        }

        /// <summary>
        /// Tests that SetExcludedWithChildren calls SetValue with correct parameters when bindable object is valid and value is null.
        /// </summary>
        [Fact]
        public void SetExcludedWithChildren_ValidBindableObjectAndNullValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bool? value = null;

            // Act
            AutomationProperties.SetExcludedWithChildren(bindable, value);

            // Assert
            bindable.Received(1).SetValue(AutomationProperties.ExcludedWithChildrenProperty, value);
        }

        /// <summary>
        /// Tests that SetExcludedWithChildren throws NullReferenceException when bindable object is null.
        /// </summary>
        [Fact]
        public void SetExcludedWithChildren_NullBindableObject_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;
            bool? value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AutomationProperties.SetExcludedWithChildren(bindable, value));
        }

        /// <summary>
        /// Tests that SetLabeledBy correctly calls SetValue with the LabeledByProperty and provided VisualElement value.
        /// This test verifies the normal execution path with valid inputs.
        /// </summary>
        [Fact]
        public void SetLabeledBy_ValidBindableObjectAndVisualElement_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();
            var visualElement = Substitute.For<VisualElement>();

            // Act
            AutomationProperties.SetLabeledBy(bindableObject, visualElement);

            // Assert
            bindableObject.Received(1).SetValue(AutomationProperties.LabeledByProperty, visualElement);
        }

        /// <summary>
        /// Tests that SetLabeledBy accepts null value parameter and passes it to SetValue correctly.
        /// This test verifies the documented behavior where null can be passed to make the bindable its own label.
        /// </summary>
        [Fact]
        public void SetLabeledBy_ValidBindableObjectWithNullValue_CallsSetValueWithNull()
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();

            // Act
            AutomationProperties.SetLabeledBy(bindableObject, null);

            // Assert
            bindableObject.Received(1).SetValue(AutomationProperties.LabeledByProperty, null);
        }

        /// <summary>
        /// Tests that SetLabeledBy throws ArgumentNullException when null bindable parameter is provided.
        /// This test verifies proper null validation for the required bindable parameter.
        /// </summary>
        [Fact]
        public void SetLabeledBy_NullBindableObject_ThrowsArgumentNullException()
        {
            // Arrange
            var visualElement = Substitute.For<VisualElement>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                AutomationProperties.SetLabeledBy(null, visualElement));

            Assert.Equal("bindable", exception.ParamName);
        }

        /// <summary>
        /// Tests that SetLabeledBy throws ArgumentNullException when both parameters are null.
        /// This test verifies that null validation occurs for the bindable parameter even when value is also null.
        /// </summary>
        [Fact]
        public void SetLabeledBy_BothParametersNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                AutomationProperties.SetLabeledBy(null, null));

            Assert.Equal("bindable", exception.ParamName);
        }

        /// <summary>
        /// Tests that SetName method calls SetValue on the bindable object with NameProperty and the provided value.
        /// Input: Valid BindableObject and string value.
        /// Expected: SetValue is called with NameProperty and the provided value.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Test Name")]
        [InlineData("Very long name with special characters !@#$%^&*()_+-=[]{}|;':\",./<>?")]
        [InlineData("Name\nWith\tSpecial\rCharacters")]
        public void SetName_ValidBindableObjectWithVariousValues_CallsSetValueWithCorrectParameters(string value)
        {
            // Arrange
            var mockBindable = Substitute.For<BindableObject>();

            // Act
            AutomationProperties.SetName(mockBindable, value);

            // Assert
            mockBindable.Received(1).SetValue(AutomationProperties.NameProperty, value);
        }

        /// <summary>
        /// Tests that SetName method throws NullReferenceException when bindable parameter is null.
        /// Input: null BindableObject and any string value.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetName_NullBindableObject_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullBindable = null;
            string testValue = "test";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AutomationProperties.SetName(nullBindable, testValue));
        }

        /// <summary>
        /// Tests that SetName method works correctly with extremely long string values.
        /// Input: Valid BindableObject and very long string value.
        /// Expected: SetValue is called with NameProperty and the long string value.
        /// </summary>
        [Fact]
        public void SetName_ValidBindableObjectWithVeryLongString_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockBindable = Substitute.For<BindableObject>();
            var longString = new string('A', 10000); // Very long string

            // Act
            AutomationProperties.SetName(mockBindable, longString);

            // Assert
            mockBindable.Received(1).SetValue(AutomationProperties.NameProperty, longString);
        }

        /// <summary>
        /// Tests that SetName method handles Unicode and international characters correctly.
        /// Input: Valid BindableObject and string with Unicode characters.
        /// Expected: SetValue is called with NameProperty and the Unicode string value.
        /// </summary>
        [Theory]
        [InlineData("测试名称")] // Chinese characters
        [InlineData("اسم الاختبار")] // Arabic characters
        [InlineData("🚀🎉🔥")] // Emoji characters
        [InlineData("Tëst Ñamé")] // Accented characters
        public void SetName_ValidBindableObjectWithUnicodeCharacters_CallsSetValueWithCorrectParameters(string value)
        {
            // Arrange
            var mockBindable = Substitute.For<BindableObject>();

            // Act
            AutomationProperties.SetName(mockBindable, value);

            // Assert
            mockBindable.Received(1).SetValue(AutomationProperties.NameProperty, value);
        }

        /// <summary>
        /// Tests that GetHelpText throws ArgumentNullException when bindable parameter is null.
        /// Verifies the method properly validates input parameters.
        /// </summary>
        [Fact]
        public void GetHelpText_NullBindable_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject bindable = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AutomationProperties.GetHelpText(bindable));
        }

        /// <summary>
        /// Tests that GetHelpText returns the help text string when a valid value is set.
        /// Verifies the method correctly retrieves and returns the help text property value.
        /// </summary>
        [Fact]
        public void GetHelpText_ValidBindableWithHelpText_ReturnsHelpText()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            var expectedHelpText = "Test help text";
            bindable.GetValue(AutomationProperties.HelpTextProperty).Returns(expectedHelpText);

            // Act
            var result = AutomationProperties.GetHelpText(bindable);

            // Assert
            Assert.Equal(expectedHelpText, result);
        }

        /// <summary>
        /// Tests that GetHelpText returns null when no help text is set on the bindable object.
        /// Verifies the method returns the default value when property is not explicitly set.
        /// </summary>
        [Fact]
        public void GetHelpText_ValidBindableWithNoHelpText_ReturnsNull()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.HelpTextProperty).Returns((string)null);

            // Act
            var result = AutomationProperties.GetHelpText(bindable);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetHelpText returns empty string when help text is explicitly set to empty.
        /// Verifies the method correctly handles empty string values.
        /// </summary>
        [Fact]
        public void GetHelpText_ValidBindableWithEmptyHelpText_ReturnsEmptyString()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            bindable.GetValue(AutomationProperties.HelpTextProperty).Returns(string.Empty);

            // Act
            var result = AutomationProperties.GetHelpText(bindable);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that GetHelpText returns whitespace string when help text contains only whitespace.
        /// Verifies the method correctly handles whitespace-only string values.
        /// </summary>
        [Fact]
        public void GetHelpText_ValidBindableWithWhitespaceHelpText_ReturnsWhitespace()
        {
            // Arrange
            var bindable = Substitute.For<BindableObject>();
            var whitespaceText = "   \t\n  ";
            bindable.GetValue(AutomationProperties.HelpTextProperty).Returns(whitespaceText);

            // Act
            var result = AutomationProperties.GetHelpText(bindable);

            // Assert
            Assert.Equal(whitespaceText, result);
        }
    }
}